using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.PlayerIdentity.Management
{
    public static class BuildHelpers
    {
        public static void CleanOldSettings<T>()
        {
            UnityEngine.Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets == null)
                return;

            var oldSettings = from s in preloadedAssets
                where s != null && s.GetType() == typeof(T)
                select s;

            if (oldSettings != null && oldSettings.Any())
            {
                var assets = preloadedAssets.ToList();
                foreach (var s in oldSettings)
                {
                    assets.Remove(s);
                }

                PlayerSettings.SetPreloadedAssets(assets.ToArray());
            }
        }
    }
}
