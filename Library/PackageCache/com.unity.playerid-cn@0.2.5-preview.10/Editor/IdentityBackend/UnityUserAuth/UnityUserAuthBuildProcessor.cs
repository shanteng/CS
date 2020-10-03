using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.PlayerIdentity.UnityUserAuth;
#if UNITY_IOS
using UnityEditor.iOS;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.PBX;
#endif
using UnityEngine;

namespace UnityEditor.PlayerIdentity.UnityUserAuth
{
    /// <summary>
    /// The build processor that makes sure that any custom configuration is passed to the
    /// UnityID Loader at runtime.
    ///
    /// Custom configuration instances that are stored in EditorBuildSettings are not copied to the target build
    /// as they are considered unreferenced assets. In order to get them to the runtime side of things, they need
    /// to be serialized to the build app and deserialized at runtime. Previously this would be a manual process
    /// requiring the implementor to manually serialize to some location that can then be read from to deserialize
    /// at runtime. With the new PlayerSettings Preloaded Assets API we can now just add our asset to the preloaded
    /// list and have it be instantiated at app launch.
    ///
    /// Note that the preloaded assets are only notified with Awake, so anything you want or need to do with the
    /// asset after launch needs to be handled there.
    ///
    /// More info on APIs used here:
    /// * <a href="https://docs.unity3d.com/ScriptReference/EditorBuildSettings.html">EditorBuildSettings</a>
    /// * <a href="https://docs.unity3d.com/ScriptReference/PlayerSettings.GetPreloadedAssets.html">PlayerSettings.GetPreloadedAssets</a>
    /// * <a href="https://docs.unity3d.com/ScriptReference/PlayerSettings.SetPreloadedAssets.html">PlayerSettings.SetPreloadedAssets</a>
    /// </summary>
    public class UnityUserAuthBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0;  }
        }

        // CleanOldSettings resets the UnityUserAuth settings stored in the Preloaded Assets 
        void CleanOldSettings()
        {
            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets == null)
                return;

            // Get the old UnityUserAuth settings from the Preloaded Assets
            var oldSettings = from s in preloadedAssets
                where s != null && s.GetType() == typeof(UnityUserAuthLoaderSettings)
                select s;

            if (oldSettings != null && oldSettings.Any())
            {
                var assets = preloadedAssets.ToList();
                foreach (var s in oldSettings)
                {
                    assets.Remove(s);
                }

                // Reset the preloaded assets to the edited set
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            // Clean and reset settings before builds to make sure we don't
            // pollute later builds with assets that may be unnecessary or are outdated.
            CleanOldSettings();

            UnityUserAuthLoaderSettings settings = UnityUserAuthLoader.GetSettings();
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
            // Clean and reset settings after builds to make sure we don't
            // pollute later builds with assets that may be unnecessary or are outdated.
            CleanOldSettings();
        }
    }
}
