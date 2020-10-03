using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEditor.PlayerIdentity.Management
{
    /// <summary>
    /// LoaderInfo is a wrapper class for loader information
    /// </summary>
    internal class LoaderInfo : IEquatable<LoaderInfo>
    {
        public Type loaderType;
        public string assetName;
        public PlayerIdentityLoader instance;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LoaderInfo && Equals((LoaderInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (loaderType != null ? loaderType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (instance != null ? instance.GetHashCode() : 0);
                return hashCode;
            }
        }

        public bool Equals(LoaderInfo other)
        {
            return other != null && loaderType == other.loaderType && Equals(instance, other.instance);
        }
    }

    /// <summary>
    /// LoaderOrderUI is the UI to decide the order of PlayerIdentityLoaders
    /// </summary>
    internal class LoaderOrderUI
    {
        private ReorderableList m_OrderedList;
        private List<LoaderInfo> m_LoadersInUse = new List<LoaderInfo>();
        private List<LoaderInfo> m_LoadersNotInUse = new List<LoaderInfo>();

        private SerializedProperty m_LoaderProperty;
        private bool m_ShouldReload = false;
        private Action m_OnUpdate;
        
        public LoaderOrderUI(List<LoaderInfo> loaderInfos, List<LoaderInfo> loadersInUse, SerializedProperty loaderProperty, Action onUpdate)
        {
            Reset(loaderInfos, loadersInUse, loaderProperty, onUpdate);
        }

        public void Reset(List<LoaderInfo> loaderInfos, List<LoaderInfo> loadersInUse, SerializedProperty loaderProperty, Action onUpdate)
        {
            m_OnUpdate = onUpdate;

            m_LoaderProperty = loaderProperty;

            m_LoadersInUse = loadersInUse;
            m_LoadersNotInUse.Clear();
            
            foreach (var info in loaderInfos)
            {
                if (!m_LoadersInUse.Contains(info))
                {
                    m_LoadersNotInUse.Add(info);
                }
            }

            m_ShouldReload = true;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            LoaderInfo info = m_LoadersInUse[index];
            var label = (info.instance == null) ? EditorGUIUtility.TrTextContent("Missing (PlayerIdentityLoader)") : EditorGUIUtility.TrTextContent(info.assetName);
            EditorGUI.LabelField(rect, label);
        }

        private float GetElementHeight(int index)
        {
            return m_OrderedList.elementHeight;
        }

        private void UpdateSerializedProperty()
        {
            if (m_LoaderProperty != null && m_LoaderProperty.isArray)
            {
                m_LoaderProperty.ClearArray();

                int index = 0;
                foreach (LoaderInfo info in m_LoadersInUse)
                {
                    m_LoaderProperty.InsertArrayElementAtIndex(index);
                    var prop = m_LoaderProperty.GetArrayElementAtIndex(index);
                    prop.objectReferenceValue = info.instance;
                    index++;
                }

                m_LoaderProperty.serializedObject.ApplyModifiedProperties();
            }

            m_OnUpdate?.Invoke();
        }

        private void ReorderLoaderList(ReorderableList list)
        {
            UpdateSerializedProperty();
        }

        private void DrawAddDropdown(Rect rect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            int index = 0;
            if(m_LoadersNotInUse.Count > 0)
            {
                foreach (var info in m_LoadersNotInUse)
                {
                    string name = info.assetName;
                    if (string.IsNullOrEmpty(name) && info.loaderType != null)
                    {
                        name = EditorUtilities.TypeNameToString(info.loaderType);
                    }

                    menu.AddItem(new GUIContent($"{index + 1}. {name}"), false, AddLoaderMenuSelected, index);
                    index++;
                }
            }

            menu.ShowAsContext();
        }

        private void AddLoaderMenuSelected(object data)
        {
            int selected = (int)data;
            LoaderInfo info = m_LoadersNotInUse[selected];

            AddLoaderMenu(info);
        }

        private void AddLoaderMenu(LoaderInfo info)
        {
            m_LoadersNotInUse.Remove(info);
            m_LoadersInUse.Add(info);
            UpdateSerializedProperty();
        }

        private void RemoveInstanceFromList(ReorderableList list)
        {
            LoaderInfo info = m_LoadersInUse[list.index];
            m_LoadersInUse.Remove(info);

            if (info.loaderType != null)
            {
                m_LoadersInUse.Remove(info);
                m_LoadersNotInUse.Add(info);
                m_ShouldReload = true;
            }
            UpdateSerializedProperty();
        }

        public bool OnGUI()
        {
            if (m_LoaderProperty == null)
                return false;

            m_ShouldReload = false;
            if (m_OrderedList == null)
            {
                m_OrderedList = new ReorderableList(m_LoadersInUse, typeof(PlayerIdentityLoader), true, true, true, true);
                m_OrderedList.drawHeaderCallback = (rect) => GUI.Label(rect, EditorGUIUtility.TrTextContent("Login Providers"), EditorStyles.label);
                m_OrderedList.drawElementCallback = (rect, index, isActive, isFocused) => DrawElementCallback(rect, index, isActive, isFocused);
                m_OrderedList.elementHeightCallback = (index) => GetElementHeight(index);
                m_OrderedList.onReorderCallback = (list) => ReorderLoaderList(list);
                m_OrderedList.onAddDropdownCallback = (rect, list) => DrawAddDropdown(rect, list);
                m_OrderedList.onRemoveCallback = (list) => RemoveInstanceFromList(list);
            }

            m_OrderedList.DoLayoutList();

            return m_ShouldReload;
        }
    }

    /// <summary>
    /// PlayerIdentityGeneralSettingsPerBuildTargetEditor is the editor for PlayerIdentityGeneralSettingsPerBuildTarget.
    /// It handles all changes to the PlayerIdentityGeneralSettings.
    /// All fields except m_LoaderManagerInstance field in PlayerIdentityGeneralSettings are global.
    /// Changes to these global fields are copied to all PlayerIdentityGeneralSettings instances.
    /// </summary>
    [CustomEditor(typeof(PlayerIdentityGeneralSettingsPerBuildTarget))]
    public class PlayerIdentityGeneralSettingsPerBuildTargetEditor : Editor
    {
        // Simple class to give updates when the asset database changes
        public class AssetCallbacks : AssetPostprocessor
        {
            static bool s_EditorUpdatable = false;
            public static System.Action Callback { get; set; }

            static AssetCallbacks()
            {
                if (!s_EditorUpdatable)
                {
                    EditorApplication.update += EditorUpdatable;
                }
                EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
            }

            static void EditorApplicationOnProjectChanged()
            {
                Callback?.Invoke();
            }

            static void EditorUpdatable()
            {
                s_EditorUpdatable = true;
                EditorApplication.update -= EditorUpdatable;
                Callback?.Invoke();
            }
        }

        /// <summary>
        /// All known backend loader info.
        /// </summary>
        private List<LoaderInfo> m_AllBackendLoaderInfo = new List<LoaderInfo>();

        /// <summary>
        /// Selected backend loader info.
        /// </summary>
        private LoaderInfo m_SelectedBackendLoaderInfo;

        /// <summary>
        /// All known provider loader info.
        /// </summary>
        private List<LoaderInfo> m_AllProviderLoaderInfo = new List<LoaderInfo>();
        
        /// <summary>
        /// Selected provider loader info.
        /// </summary>
        private List<LoaderInfo> m_SelectedProviderLoaderInfos = new List<LoaderInfo>();
        
        /// <summary>
        /// Helper class for caching per target group UI objects. 
        /// </summary>
        private class PerBuildTargetGroupCache
        {
            internal LoaderOrderUI loaderOrderUI;
            internal SerializedObject generalSettingsObject;
            internal SerializedObject managerSettingsObject;
        }

        private Dictionary<BuildTargetGroup, PerBuildTargetGroupCache> m_CachedLoadOrderUIs = new Dictionary<BuildTargetGroup, PerBuildTargetGroupCache>();
        
        private bool m_MustReloadData = true;
        
        private void AssetProcessorCallback()
        {
            m_MustReloadData = true;
        }

        public void OnEnable()
        {
            AssetCallbacks.Callback += AssetProcessorCallback;
            ReloadData();
        }

        public void OnDisable()
        {
            AssetCallbacks.Callback -= AssetProcessorCallback;
        }

        void ReloadData()
        {
            m_AllBackendLoaderInfo.Clear();
            m_SelectedBackendLoaderInfo = null;
            m_AllProviderLoaderInfo.Clear();
            m_SelectedProviderLoaderInfos.Clear();

            PlayerIdentitySettingsManager.PopulateAllKnownLoaderInfos(m_AllBackendLoaderInfo, m_AllProviderLoaderInfo);

            m_MustReloadData = false;
        }

        public override void OnInspectorGUI()
        {
            PlayerIdentityGeneralSettingsPerBuildTarget targetObject;
            if (serializedObject == null || serializedObject.targetObject == null)
            {
                return;
            }

            if (m_MustReloadData)
            {
                ReloadData();
            }

            serializedObject.Update();

            // Load the current settings
            targetObject = serializedObject.targetObject as PlayerIdentityGeneralSettingsPerBuildTarget;
            if (targetObject == null)
            {
                return;
            }
            // Always use standalone for setting identity backend and other global settings
            var standaloneCurrentSettings = targetObject.SettingsForBuildTarget(BuildTargetGroup.Standalone);

            // UI to select backend loader
            GUILayout.Label("Select Identity Backend", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Backend Type");

                var currentSelectedBackendIndex =
                    GetSelectedBackendLoaderIndex(standaloneCurrentSettings, m_AllBackendLoaderInfo);
                currentSelectedBackendIndex =
                    EditorGUILayout.Popup(currentSelectedBackendIndex, GetLoaderNames(m_AllBackendLoaderInfo));
                if (m_AllBackendLoaderInfo.Count > 0)
                {
                    m_SelectedBackendLoaderInfo = m_AllBackendLoaderInfo[currentSelectedBackendIndex];

                    // Apply to all values, not just current setting
                    ApplyChangesToAllSettings((group, currentValue) =>
                    {
                        var localCachedItem = GetOrCreateCachedItem(group, currentValue.objectReferenceValue as PlayerIdentityGeneralSettings);
                        localCachedItem.managerSettingsObject.FindProperty("m_BackendLoader").objectReferenceValue =
                            m_SelectedBackendLoaderInfo.instance;
                        localCachedItem.managerSettingsObject.ApplyModifiedProperties();
                    });
                }
                else
                {
                    m_SelectedBackendLoaderInfo = null;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            // UI to select identity providers
            GUILayout.Label("Configure Identity Providers", EditorStyles.boldLabel);
            var buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
            {
                var currentSettings = targetObject.SettingsForBuildTarget(buildTargetGroup);
                var cachedItem = GetOrCreateCachedItem(buildTargetGroup, currentSettings);
                if (cachedItem.loaderOrderUI == null)
                {
                    var loaderList = FindLoaderList(cachedItem);
                    var loadersInUse = FindLoadersInUse(cachedItem);
                    cachedItem.loaderOrderUI = new LoaderOrderUI(m_AllProviderLoaderInfo, loadersInUse, loaderList,
                        onUpdate: () =>
                        {
                            ReloadData();
                        });
                }

                m_MustReloadData = cachedItem.loaderOrderUI.OnGUI();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndBuildTargetSelectionGrouping();
            
            // Options
            // Whether verify email shall be triggered automatically
            standaloneCurrentSettings.Manager.automaticallyVerifyEmail = EditorGUILayout.ToggleLeft("Automatically send verification email to users who sign up with email.", standaloneCurrentSettings.Manager.automaticallyVerifyEmail);
            
            // Apply to all values
            ApplyChangesToAllSettings((group, currentValue) =>
            {
                var localCachedItem = GetOrCreateCachedItem(group, currentValue.objectReferenceValue as PlayerIdentityGeneralSettings);
                localCachedItem.managerSettingsObject.FindProperty("m_AutomaticallyVerifyEmail").boolValue =
                    standaloneCurrentSettings.Manager.automaticallyVerifyEmail;
                localCachedItem.managerSettingsObject.ApplyModifiedProperties();
            });
            
            serializedObject.ApplyModifiedProperties();
        }

        private string[] GetLoaderNames(List<LoaderInfo> loaderInfos)
        {
            return loaderInfos.Select(x => x.instance.name).ToArray();
        }

        private static int GetSelectedBackendLoaderIndex(PlayerIdentityGeneralSettings settings, List<LoaderInfo> backendLoaders)
        {
            if (settings.Manager == null || settings.Manager.backendLoader == null)
            {
                return 0;
            }

            var loaderName = settings.Manager.backendLoader.name;
            int index = backendLoaders.FindIndex(x => x.instance != null && x.instance.name == loaderName);
            return index < 0 ? 0 : index;
        }

        private SerializedProperty FindLoaderList(PerBuildTargetGroupCache cachedItem)
        {
            return cachedItem.managerSettingsObject.FindProperty("m_ProviderLoaders");
        }

        private List<LoaderInfo> FindLoadersInUse(PerBuildTargetGroupCache cachedItem)
        {
            var providerLoadersProperty = cachedItem.managerSettingsObject.FindProperty("m_ProviderLoaders");

            List<LoaderInfo> loaderInUse = new List<LoaderInfo>();
            for (int i = 0; i < providerLoadersProperty.arraySize; ++i)
            {
                var loaderInstance = providerLoadersProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                var loaderInfo = m_AllProviderLoaderInfo.FirstOrDefault(x => x.instance == loaderInstance);
                if (loaderInfo != null)
                {
                    loaderInUse.Add(loaderInfo);
                }
            }

            return loaderInUse;
        }

        private void ApplyChangesToAllSettings(Action<BuildTargetGroup, SerializedProperty> action)
        {
            var keys = serializedObject.FindProperty("m_Keys");
            var values = serializedObject.FindProperty("m_Values");
            var enumValues = Enum.GetValues(typeof(BuildTargetGroup));
            for (int i = 0; i < keys.arraySize; ++i)
            {
                var targetGroupKey = (BuildTargetGroup) enumValues.GetValue(keys.GetArrayElementAtIndex(i).enumValueIndex);
                var value = values.GetArrayElementAtIndex(i);
                action(targetGroupKey, value);
            }
        }

        private PerBuildTargetGroupCache GetOrCreateCachedItem(BuildTargetGroup group, PlayerIdentityGeneralSettings currentSettings)
        {
            PerBuildTargetGroupCache cachedItem;
            m_CachedLoadOrderUIs.TryGetValue(group, out cachedItem);

            if (cachedItem == null)
            {
                // Initialize the item
                cachedItem = new PerBuildTargetGroupCache { generalSettingsObject = new SerializedObject(currentSettings) };
                var managerSettingsProperty = cachedItem.generalSettingsObject.FindProperty("m_LoaderManagerInstance");
                if (managerSettingsProperty.objectReferenceValue == null)
                {
                    // Create the manager settings
                    var ms = CreateInstance<PlayerIdentityManagerSettings>();
                    AssetDatabase.AddObjectToAsset(ms, AssetDatabase.GetAssetOrScenePath(currentSettings));
                    managerSettingsProperty.objectReferenceValue = ms;
                    cachedItem.generalSettingsObject.ApplyModifiedProperties();
                }
                cachedItem.managerSettingsObject = new SerializedObject(managerSettingsProperty.objectReferenceValue);

                if (m_SelectedBackendLoaderInfo != null)
                {
                    cachedItem.managerSettingsObject.FindProperty("m_BackendLoader").objectReferenceValue =
                        m_SelectedBackendLoaderInfo.instance;                    
                }
                m_CachedLoadOrderUIs[group] = cachedItem;
            }
            else
            {
                cachedItem.generalSettingsObject.Update();
                cachedItem.managerSettingsObject.Update();
            }

            return cachedItem;
        }
    }
}
