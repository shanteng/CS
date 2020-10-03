using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEditor.PlayerIdentity.Management;
using UnityEngine;
using UnityEngine.PlayerIdentity.Apple;
using UnityEngine.PlayerIdentity.Management;

namespace UnityEditor.PlayerIdentity.Apple
{
    internal class ModifyAndroidManifest : IPostGenerateGradleAndroidProject
    {
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Android)
                return;

            var identitySettings = GetPlayerIdentityGeneralSettings(EditorUserBuildSettings.selectedBuildTargetGroup);
            var appleLoaderExists =
                identitySettings?.Manager?.providerLoaders?.Exists(x => x.GetType() == typeof(AppleLoader));
            if (appleLoaderExists == null || !(bool)appleLoaderExists)
            {
                return;
            }

            var androidManifest = new AndroidManifest(GetManifestPath(basePath));
            AddAppleJavaCode(basePath);

            // Modify android manifest file to include the activity for custom tab-based Apple Sign-in
            androidManifest.AddSignInWithAppleActivity();
            
            androidManifest.Save();
        }

        public int callbackOrder => 1;

        private string m_ManifestFilePath;

        private string addAppleCodePath;

        private string GetManifestPath(string basePath)
        {
            if (string.IsNullOrEmpty(m_ManifestFilePath))
            {
                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                m_ManifestFilePath = pathBuilder.ToString();
            }

            return m_ManifestFilePath;
        }

        private string GetAddAppleCodePath()
        {
           var path = Path.GetDirectoryName(m_ManifestFilePath);
           var pathBuilder = new StringBuilder(path);
           pathBuilder.Append(Path.DirectorySeparatorChar).Append("java");
           pathBuilder.Append(Path.DirectorySeparatorChar).Append("com");
           pathBuilder.Append(Path.DirectorySeparatorChar).Append("unity3d");
           pathBuilder.Append(Path.DirectorySeparatorChar).Append("playeridentity");
           addAppleCodePath = pathBuilder.ToString();

           return addAppleCodePath;
        }

        private PlayerIdentityGeneralSettings GetPlayerIdentityGeneralSettings(BuildTargetGroup targetGroup)
        {
            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out buildTargetSettings);
            return buildTargetSettings == null ? null : buildTargetSettings.SettingsForBuildTarget(targetGroup);
        }
        
        private void AddAppleJavaCode(string path)
        {
            var basePath =GetAddAppleCodePath() ;
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string[] filename =
            {
                "SignInWithApple", "SignInWithAppleIntentFilter", "SignInWithAppleResult", "SignInWithAppleCallbacks"
            };
            
            for (int i = 0; i < filename.Length; i++)
            {
               var code=(TextAsset)AssetDatabase.LoadAssetAtPath("Packages/com.unity.playerid-cn/Editor/IdentityProvider/Apple/Resources/Android/"+filename[i]+".txt", typeof(TextAsset));
               var codePath = basePath + "/"+filename[i]+".java";
                if (!File.Exists(codePath)) 
                {    
                    if (code != null)
                    {
                        var generatedCode = code.text;
                        var writer = new StreamWriter(codePath, false);
                        writer.Write(generatedCode);
                        writer.Close();
                    }
                }
            }
        }
        
    }

    internal class AndroidXmlDocument : XmlDocument
    {
        private string m_Path;
        protected XmlNamespaceManager m_NsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }

            m_NsMgr = new XmlNamespaceManager(NameTable);
            m_NsMgr.AddNamespace("android", AndroidXmlNamespace);
        }

        public string Save()
        {
            return SaveAs(m_Path);
        }

        public string SaveAs(string path)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }

            return path;
        }
    }


    internal class AndroidManifest : AndroidXmlDocument
    {
        private readonly XmlElement m_ApplicationElement;

        public AndroidManifest(string path) : base(path)
        {
            m_ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        internal void AddSignInWithAppleActivity()
        {
            XmlElement activity = CreateElement("activity");
            activity.Attributes.Append(CreateAndroidAttribute("name",
                "com.unity3d.playeridentity.SignInWithAppleIntentFilter"));
            activity.Attributes.Append(CreateAndroidAttribute("theme", "@android:style/Theme.Translucent.NoTitleBar"));
            activity.Attributes.Append(CreateAndroidAttribute("noHistory", "true"));
            activity.Attributes.Append(CreateAndroidAttribute("launchMode", "singleTask"));

            XmlElement intentFilter = CreateElement("intent-filter");
            activity.AppendChild(intentFilter);

            XmlElement data = CreateElement("data");
            var settings = AppleLoader.GetSettings();
            var deepLinkUri = new Uri(settings.m_ApplicationDeepLink);
            data.Attributes.Append(CreateAndroidAttribute("scheme", deepLinkUri.Scheme));
            data.Attributes.Append(CreateAndroidAttribute("host", deepLinkUri.Host));
            intentFilter.AppendChild(data);

            XmlElement action = CreateElement("action");
            action.Attributes.Append(CreateAndroidAttribute("name", "android.intent.action.VIEW"));
            intentFilter.AppendChild(action);

            XmlElement category1 = CreateElement("category");
            category1.Attributes.Append(CreateAndroidAttribute("name", "android.intent.category.DEFAULT"));
            intentFilter.AppendChild(category1);

            XmlElement category2 = CreateElement("category");
            category2.Attributes.Append(CreateAndroidAttribute("name", "android.intent.category.BROWSABLE"));
            intentFilter.AppendChild(category2);

            m_ApplicationElement.AppendChild(activity);
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value)
        {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }
    }
}
