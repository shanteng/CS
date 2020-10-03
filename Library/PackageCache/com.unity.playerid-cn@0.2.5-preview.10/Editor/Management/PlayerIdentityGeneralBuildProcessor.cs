using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEditor.PlayerIdentity.Management
{
    class PlayerIdentityGeneralBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0;  }
        }

        void CleanOldSettings()
        {
            BuildHelpers.CleanOldSettings<PlayerIdentityGeneralSettings>();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Always remember to cleanup preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();

            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return;

            PlayerIdentityGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(report.summary.platformGroup);
            if (settings == null)
                return;

            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();

            if (!preloadedAssets.Contains(settings))
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Always remember to cleanup preloaded assets after build to make sure we don't
            // dirty later builds with assets that may not be needed or are out of date.
            CleanOldSettings();
        }
    }
}
