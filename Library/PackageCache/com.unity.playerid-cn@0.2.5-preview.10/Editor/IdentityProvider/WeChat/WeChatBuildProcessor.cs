using System.IO;
using System.Linq;

using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.PlayerIdentity.WeChat;
#if UNITY_IOS || UNITY_ANDROID
    using UnityEditor.Callbacks; 
#endif
#if UNITY_IOS || UNITY_TVOS
    using UnityEditor.iOS.Xcode;
#endif

namespace UnityEditor.PlayerIdentity.WeChat
{
    public class WeChatBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        // CleanOldSettings resets the WeChat settings stored in the Preloaded Assets 
        void CleanOldSettings()
        {
            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets == null)
                return;

            // Get the old WeChat settings from the Preloaded Assets
            var oldSettings = from s in preloadedAssets
                where s != null && s.GetType() == typeof(WeChatLoaderSettings)
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

            WeChatLoaderSettings settings = WeChatLoader.GetSettings();
            if (settings == null)
                return;

            Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (!preloadedAssets.Contains(settings))
            {
                var assets = preloadedAssets.ToList();
                assets.Add(settings);
                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }


#if UNITY_IOS || UNITY_TVOS
        private const string UNITY_PACKAGE_NAME = "com.unity.playerid-cn";
        private string GenerateIOSCallbackCode(BuildReport report)
        {
            var pluginPath = report.summary.outputPath + $"/Libraries/{UNITY_PACKAGE_NAME}/Runtime/Login/Apple";
            if (!Directory.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
            }
            var basePath = pluginPath + "/iOS";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            var codePath = basePath + "/CustomAppController.mm";
            if (File.Exists(codePath)) // don't overwrite
            {
                return "";
            }
            var code = Resources.Load<TextAsset>("CustomAppController");
            if (code != null)
            {
                var writer = new StreamWriter(codePath, false);
                writer.Write(code.text);
                writer.Close();
            }
            return $"Libraries/{UNITY_PACKAGE_NAME}/Runtime/Login/Apple/iOS/CustomAppController.mm";
        }
#endif

        public void OnPostprocessBuild(BuildReport report)
        {
            // Clean and reset settings after builds to make sure we don't
            // pollute later builds with assets that may be unnecessary or are outdated.
            CleanOldSettings();
#if (UNITY_IOS || UNITY_TVOS)

            PBXProject proj = new PBXProject();
            string pbxFilename = report.summary.outputPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            proj.ReadFromFile(pbxFilename);
#if UNITY_2019_2
            var targetName = PBXProject.GetUnityTargetName();
            var targetBuild = proj.TargetGuidByName(name: targetName);
#endif
#if UNITY_2019_3_OR_NEWER
            // The framework needs to be on the framework target rather than the main target as the capability manager does
            string targetBuild = proj.GetUnityFrameworkTargetGuid();
#endif
            //lib path
//            proj.AddBuildProperty(targetBuild, "LIBRARY_SEARCH_PATH", "$(PROJECT_DIR)/Runtime/Login/WeChat/Apple/iOS");
            var codePath = GenerateIOSCallbackCode(report);
            if (!string.IsNullOrEmpty(codePath))
            {
                proj.AddFileToBuild(targetBuild, proj.AddFile(codePath, codePath));

            }
            

            // The framework
            proj.AddFrameworkToProject(targetBuild, "libc++.tbd", false);
            proj.AddFrameworkToProject(targetBuild, "libz.tbd", false);
            proj.AddFrameworkToProject(targetBuild, "libsqlite3.tbd", false);
            proj.AddFrameworkToProject(targetBuild, "CoreTelephony.framework", false);


            proj.AddBuildProperty(targetBuild, "OTHER_LDFLAGS", "-all_load");
            proj.AddBuildProperty(targetBuild, "OTHER_LDFLAGS", "-ObjC");
            proj.WriteToFile(pbxFilename);
#endif
         }

#if UNITY_IOS
        [PostProcessBuild]
        public static void ChangeXCodePlist(BuildTarget buildTarget, string pathToBuildProject)
        {
            if (buildTarget != BuildTarget.iOS) return;
            
            WeChatLoaderSettings settings = WeChatLoader.GetSettings();
            if (settings == null)
            {
                return;
            }
            var appId = settings.m_AppID;
            if (string.IsNullOrEmpty(appId))
            {
                return;
            }
            
            // Info.plist
            var plistPath = pathToBuildProject + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // ROOT
            var rootDict = plist.root;
            const string urlTypesKey = "CFBundleURLTypes";
            PlistElementArray urlTypes = (PlistElementArray) rootDict[urlTypesKey];
            if (null == urlTypes)
            {
                urlTypes = rootDict.CreateArray(urlTypesKey);
            }

            // Add URLScheme for WeChat
            var wxUrl = urlTypes.AddDict();
            wxUrl.SetString("CFBundleTyeRole", "Editor");
            wxUrl.SetString("CFBundleURLName", "weixin");
            wxUrl.SetString("CFBundleURLSchemes", appId);
            var wxUrlScheme = wxUrl.CreateArray("CFBundleURLSchemes");
            wxUrlScheme.AddString(appId);
            
            
            //whitelist WeChat w
            const string queriesSchemesKey = "LSApplicationQueriesSchemes";
            PlistElementArray queriesSchemes = (PlistElementArray) rootDict[queriesSchemesKey];
            if (null == queriesSchemes)
            {
                queriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            }

            queriesSchemes.AddString("wechat");
            queriesSchemes.AddString("weixin");
            queriesSchemes.AddString(val: appId);
            
            File.WriteAllText(plistPath, plist.WriteToString());
        }
#endif
        
    }
}
