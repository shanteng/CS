using System;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEditor.PlayerIdentity.Management
{
    public static class PlayerIdentityGeneralSettingsUpgrade
    {
        public static bool UpgradeSettingsToPerBuildTarget(string path)
        {
            var generalSettings = GetPlayerIdentityGeneralSettingsInstance(path);
            if (generalSettings == null)
                return false;

            if (!AssetDatabase.IsMainAsset(generalSettings))
                return false;

            PlayerIdentityGeneralSettings newSettings = ScriptableObject.CreateInstance<PlayerIdentityGeneralSettings>() as PlayerIdentityGeneralSettings;
            newSettings.Manager = generalSettings.Manager;
            generalSettings = null;

            AssetDatabase.DeleteAsset(path);

            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = ScriptableObject.CreateInstance<PlayerIdentityGeneralSettingsPerBuildTarget>() as PlayerIdentityGeneralSettingsPerBuildTarget;            
            AssetDatabase.CreateAsset(buildTargetSettings, path);

            buildTargetSettings.SetSettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup, newSettings);
            AssetDatabase.AddObjectToAsset(newSettings, path);
            AssetDatabase.SaveAssets();

            Debug.LogWarningFormat("PlayerIdentity General Settings have been upgraded to be per-Build Target Group. Original settings were moved to Build Target Group {0}.", EditorUserBuildSettings.selectedBuildTargetGroup);
            return true;
        }

        private static PlayerIdentityGeneralSettings GetPlayerIdentityGeneralSettingsInstance(string pathToSettings)
        {
            PlayerIdentityGeneralSettings ret = null;
            if (pathToSettings.Length > 0)
            {
                ret = AssetDatabase.LoadAssetAtPath(pathToSettings, typeof(PlayerIdentityGeneralSettings)) as PlayerIdentityGeneralSettings;
            }

            return ret;
        }
    }
}
