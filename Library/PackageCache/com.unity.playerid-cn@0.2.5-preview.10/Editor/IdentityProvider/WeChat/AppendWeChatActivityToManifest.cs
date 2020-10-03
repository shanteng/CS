using System;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;
using UnityEditor.PlayerIdentity.Management;
using UnityEngine;
using UnityEngine.PlayerIdentity.Management;
using UnityEngine.PlayerIdentity.WeChat;

namespace UnityEditor.PlayerIdentity.WeChat
{
    public class AppendWeChatActivityToManifest : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1;
        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Android)
                return;

            var settings = WeChatLoader.GetSettings();
            if (settings == null)
            {
                return;
            }
            if (!settings.m_GenerateCallbackCode)
            {
                return;
            }

            var androidManifest = new WeChatAndroidManifest(GetManifestPath(basePath));

            // Modify android manifest file to include the activity for customtab based WeChat Sign-in
            androidManifest.AddSignInWithWeChatActivity();
            
            androidManifest.Save();
        }
        
        private PlayerIdentityGeneralSettings GetPlayerIdentityGeneralSettings(BuildTargetGroup targetGroup)
        {
            PlayerIdentityGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(PlayerIdentityGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return null;

            return buildTargetSettings.SettingsForBuildTarget(targetGroup);
        }
        
        private string _manifestFilePath;

        
        private string GetManifestPath(string basePath)
        {
            if (string.IsNullOrEmpty(_manifestFilePath))
            {
                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();
            }

            return _manifestFilePath;
        }
    }
    
    internal class AndroidXmlDocument : XmlDocument
    {
        private string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }

            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
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
    
    internal class WeChatAndroidManifest : AndroidXmlDocument
    {
        private readonly XmlElement ApplicationElement;

        public WeChatAndroidManifest(string path) : base(path)
        {
            ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        internal void AddSignInWithWeChatActivity()
        {
            XmlElement activity = CreateElement("activity");
            activity.Attributes.Append(CreateAndroidAttribute("name",
                Application.identifier + ".wxapi.WXEntryActivity"));
            activity.Attributes.Append(CreateAndroidAttribute("theme", "@android:style/Theme.Translucent.NoTitleBar"));
            activity.Attributes.Append(CreateAndroidAttribute("exported", "true"));
            activity.Attributes.Append(CreateAndroidAttribute("launchMode", "singleTask"));

            ApplicationElement.AppendChild(activity);
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value)
        {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }
    }
}