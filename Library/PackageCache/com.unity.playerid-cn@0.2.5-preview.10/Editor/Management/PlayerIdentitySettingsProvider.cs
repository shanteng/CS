using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;
using Logger = UnityEngine.PlayerIdentity.Utils.Logger;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityEditor.PlayerIdentity.Management
{
    /// <summary>
    /// PlayerIdentitySettingsManager is the setting provider that is responsible for
    /// the "Player Identity" item in project settings window
    /// </summary>
    internal class PlayerIdentitySettingsManager : SettingsProvider
    {
        private static string s_SettingsRootTitle = "Project/Player Identity";

        /// <summary>
        /// s_SettingsManager is the singleton of this class
        /// </summary>
        private static PlayerIdentitySettingsManager s_SettingsManager;

        
        /// <summary>
        /// m_SettingsWrapper is the wrapper of currentSettings to bind with editor UI components
        /// </summary>
        private SerializedObject m_SettingsWrapper;

        private Editor CachedEditor;

        /// <summary>
        /// currentSettings reads the current settings from EditorBuildSettings.
        /// If not found, then it will try to find it from assets and put to EditorBuildSettings.
        /// If it's not found again, it will try to create it at the default location and put to EditorBuildSettings.
        /// </summary>
        internal static PlayerIdentityGeneralSettingsPerBuildTarget currentSettings
        {
            get
            {
                PlayerIdentityGeneralSettingsPerBuildTarget generalSettings = null;
                EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out generalSettings);
                if (generalSettings != null)
                {
                    return generalSettings;
                }
                
                // Try to search for the PlayerIdentityGeneralSettings from asset database by type
                string searchText = "t:PlayerIdentityGeneralSettings";
                string[] assets = AssetDatabase.FindAssets(searchText);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(PlayerIdentityGeneralSettingsPerBuildTarget)) as PlayerIdentityGeneralSettingsPerBuildTarget;
                }

                if (generalSettings == null)
                {
                    // If not found, try to create one in the default path
                    generalSettings = ScriptableObject.CreateInstance(typeof(PlayerIdentityGeneralSettingsPerBuildTarget)) as PlayerIdentityGeneralSettingsPerBuildTarget;
                    string assetPath = EditorUtilities.GetAssetPathForComponents(EditorUtilities.defaultGeneralSettingsPath);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        assetPath = Path.Combine(assetPath, "PlayerIdentityGeneralSettings.asset");
                        AssetDatabase.CreateAsset(generalSettings, assetPath);
                    }
                }
                EditorBuildSettings.AddConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, generalSettings, true);
                
                return generalSettings;
            }
        }
        
        [InitializeOnLoadMethod]
        internal static void CreatingSettingsOnLoad()
        {
            var _ = currentSettings;  // for side effect only
        }
        
        [UnityEngine.Internal.ExcludeFromDocs]
        PlayerIdentitySettingsManager(string path, SettingsScope scopes = SettingsScope.Project) : base(path, scopes)
        {
        }

        [SettingsProvider]
        [UnityEngine.Internal.ExcludeFromDocs]
        static SettingsProvider Create()
        {
            return s_SettingsManager ?? (s_SettingsManager = new PlayerIdentitySettingsManager(s_SettingsRootTitle));
        }

        [SettingsProviderGroup]
        [UnityEngine.Internal.ExcludeFromDocs]
        static SettingsProvider[] CreateAllChildSettingsProviders()
        {
            List<SettingsProvider> ret = new List<SettingsProvider>();
            if (s_SettingsManager != null)
            {
                var ats = TypeLoaderExtensions.GetAllTypesWithAttribute<PlayerIdentityConfigurationDataAttribute>();
                foreach (var at in ats)
                {
                    var xrbda = at.GetCustomAttributes(typeof(PlayerIdentityConfigurationDataAttribute), true)[0] as PlayerIdentityConfigurationDataAttribute;
                    string settingsPath = $"{s_SettingsRootTitle}/{xrbda.displayName}";
                    var resProv = new PlayerIdentityConfigurationProvider(settingsPath, xrbda.buildSettingsKey, at);
                    ret.Add(resProv);
                }
            }

            return ret.ToArray();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var settings = currentSettings;
            if (settings != null)
            {
                m_SettingsWrapper = new SerializedObject(settings);
            }
        }

        public override void OnDeactivate()
        {
            m_SettingsWrapper = null;
            CachedEditor = null;
        }

        public override void OnGUI(string searchContext)
        {
            if (m_SettingsWrapper != null  && m_SettingsWrapper.targetObject != null)
            {
                m_SettingsWrapper.Update();

                if (CachedEditor == null)
                {
                    CachedEditor = Editor.CreateEditor(currentSettings);
                }
                CachedEditor.OnInspectorGUI();
            }

            base.OnGUI(searchContext);
        }

        internal static void PopulateAllKnownLoaderInfos(List<LoaderInfo> backendLoaders, List<LoaderInfo> providerLoaders)
        {
            var loaderTypes = TypeLoaderExtensions.GetAllTypesWithInterface<PlayerIdentityLoader>();
            foreach (Type loaderType in loaderTypes)
            {
                if (string.Compare("DummyLoader", loaderType.Name, StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare("SampleLoader", loaderType.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                var assets = AssetDatabase.FindAssets($"t:{loaderType}");
                if (!assets.Any())
                {
                    // Create the instance and asset
                    var newAssetName = $"{EditorUtilities.TypeNameToString(loaderType)}.asset";
                    var loader = Editor.CreateInstance(loaderType) as PlayerIdentityLoader;
                    string assetPath = EditorUtilities.GetAssetPathForComponents(EditorUtilities.defaultLoaderPath);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        continue;
                    }

                    assetPath = Path.Combine(assetPath, newAssetName);
                    AssetDatabase.CreateAsset(loader, assetPath);

                    assets = AssetDatabase.FindAssets($"t:{loaderType}");
                } 

                // Add the loader info only if the loader is created.
                foreach (var asset in assets)
                {
                    string path = AssetDatabase.GUIDToAssetPath(asset);

                    LoaderInfo info = new LoaderInfo();
                    info.loaderType = loaderType;
                    info.instance = AssetDatabase.LoadAssetAtPath(path, loaderType) as PlayerIdentityLoader;
                    if (info.instance == null)
                    {
                        continue;
                    }
                    info.assetName = Path.GetFileNameWithoutExtension(path);

                    switch (info.instance.Category)
                    {
                        case PlayerIdentityLoaderCategory.IdentityBackend:
                            backendLoaders.Add(info);
                            break;
                        case PlayerIdentityLoaderCategory.IdentityProvider:
                            providerLoaders.Add(info);
                            break;
                        default:
                            Logger.Warning("Unknown identity loader category: " + info.instance.Category);
                            break;
                    }
                }
            }
        }
    }
}
