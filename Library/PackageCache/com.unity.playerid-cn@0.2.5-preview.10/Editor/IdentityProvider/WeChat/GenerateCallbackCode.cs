using System.IO;
using System.Text;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.PlayerIdentity.WeChat;

namespace UnityEditor.PlayerIdentity.WeChat
{
    public class GenerateCallbackCode : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 2;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Android)
                return;
            var settings = WeChatLoader.GetSettings();
            if (settings == null)
            {
                return;
            }
            if (settings.m_GenerateCallbackCode)
            {
                GenerateAndroidCallbackCode(path);
            }
        }
        
        private void GenerateAndroidCallbackCode(string proejectPath)
        {
            
            var basePath = GetPackagePath(proejectPath, Application.identifier) + "/wxapi";
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var codePath = basePath + "/WXEntryActivity.java";
            if (File.Exists(codePath)) // do not overwrite what has been done by developer
            {    
                return;
            }
            
            var code = Resources.Load<TextAsset>("WxLoginCallBack");
            if (code != null)
            {
                
                
                var generatedCode = code.text.Replace("com.unity.EndlessRunnerSampleGame.TkeDemo", Application.identifier);
                var writer = new StreamWriter(codePath, false);
                writer.Write(generatedCode);
                writer.Close();
            }
        }
        
        private string GetPackagePath(string basePath, string package)
        {
            var pathBuilder = new StringBuilder(basePath);
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
            pathBuilder.Append(Path.DirectorySeparatorChar).Append("java");
            
            var codePath = package.Split('.');
            foreach (var p in codePath)
            {
                pathBuilder.Append(Path.DirectorySeparatorChar).Append(p);
            }

            return pathBuilder.ToString();
        }
    }
}