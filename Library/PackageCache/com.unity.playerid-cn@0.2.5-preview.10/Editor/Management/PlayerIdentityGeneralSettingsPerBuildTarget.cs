using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEditor.PlayerIdentity.Management
{
    /// <summary>
    /// PlayerIdentityGeneralSettingsPerBuildTarget is the per build target settings.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
   public class PlayerIdentityGeneralSettingsPerBuildTarget : ScriptableObject, ISerializationCallbackReceiver
   {
        [SerializeField]
        private List<BuildTargetGroup> m_Keys = new List<BuildTargetGroup>();

        [SerializeField]
        private List<PlayerIdentityGeneralSettings> m_Values = new List<PlayerIdentityGeneralSettings>();
        
        /// <summary>
        /// Settings is not serialized. It's the convenient property to access the per platform PlayerIdentityGeneralSettings.
        /// </summary>
        private Dictionary<BuildTargetGroup, PlayerIdentityGeneralSettings> m_Settings = new Dictionary<BuildTargetGroup, PlayerIdentityGeneralSettings>();

#if UNITY_EDITOR
        // Simple class to give updates when the asset database changes
        class AssetCallbacks : AssetPostprocessor
        {
            static bool s_Upgrade = true;
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (s_Upgrade)
                {
                    s_Upgrade = false;
                    BeginUpgradeSettings();
                }
            }

            static void BeginUpgradeSettings()
            {
                const string searchText = "t:PlayerIdentityGeneralSettings";
                string[] assets = AssetDatabase.FindAssets(searchText);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    PlayerIdentityGeneralSettingsUpgrade.UpgradeSettingsToPerBuildTarget(path);
                }
            }
        }

        static PlayerIdentityGeneralSettingsPerBuildTarget()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AttemptInitializePlayerIdentitySdkBeforePlayModeStarted()
        {
            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return;

            PlayerIdentityGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (instance == null || !instance.InitManagerOnStart)
                return;

            instance.InternalPlayModeStateChanged(PlayModeStateChange.EnteredPlayMode);
        }


        static void PlayModeStateChanged(PlayModeStateChange state)
        {
            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return;

            PlayerIdentityGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (instance == null || !instance.InitManagerOnStart)
                return;

            instance.InternalPlayModeStateChanged(state);
        }
#endif

        public void SetSettingsForBuildTarget(BuildTargetGroup targetGroup, PlayerIdentityGeneralSettings settings)
        {
            m_Settings[targetGroup] = settings;
        }

        public PlayerIdentityGeneralSettings SettingsForBuildTarget(BuildTargetGroup targetGroup)
        {
            PlayerIdentityGeneralSettings ret = null;
            m_Settings.TryGetValue(targetGroup, out ret);
            
           
            if (ret == null)
            {
                ret = CreateInstance<PlayerIdentityGeneralSettings>();
                SetSettingsForBuildTarget(targetGroup, ret);
                AssetDatabase.AddObjectToAsset(ret, AssetDatabase.GetAssetOrScenePath(this));
                
                if (ret.Manager == null)
                {
                    var ms = CreateInstance<PlayerIdentityManagerSettings>();
                    AssetDatabase.AddObjectToAsset(ms, AssetDatabase.GetAssetOrScenePath(ret));
                    ret.Manager = ms;
                    
                    List<LoaderInfo> m_AllBackendLoaderInfo = new List<LoaderInfo>();
                    List<LoaderInfo> m_AllProviderLoaderInfo = new List<LoaderInfo>();
                    PlayerIdentitySettingsManager.PopulateAllKnownLoaderInfos(m_AllBackendLoaderInfo, m_AllProviderLoaderInfo);
                    LoaderInfo defaultbackloader = null;
                    if (m_AllBackendLoaderInfo.Count != 0)
                    {
                        defaultbackloader = m_AllBackendLoaderInfo[0];
                    }

                    if (defaultbackloader != null)
                    {
                        ret.Manager.backendLoader = defaultbackloader.instance;
                    }
                     
                }

            }

            return ret;
        }

        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();

            foreach (var kv in m_Settings)
            {
                m_Keys.Add(kv.Key);
                m_Values.Add(kv.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            m_Settings = new Dictionary<BuildTargetGroup, PlayerIdentityGeneralSettings>();
            for (int i = 0; i < Math.Min(m_Keys.Count, m_Values.Count); i++)
            {
                m_Settings.Add(m_Keys[i], m_Values[i]);
            }
        }
   }
}
