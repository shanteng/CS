using System;
using UnityEngine;

namespace UnityEngine.PlayerIdentity.UnityUserAuth
{
    /// <summary>
    /// IPersistentService is an interface that allows UnityUserAuth to
    /// persist the current session info
    /// The implementation shall keep the data in the private storage of the app
    /// </summary>
    public interface IPersistentService
    {
        /// <summary>
        /// Read a value by key
        /// </summary>
        /// <param name="key">The key to read</param>
        /// <returns>The data by the key</returns>
        string ReadValue(string key);

        /// <summary>
        /// Set the value for a key
        /// </summary>
        /// <param name="key">The key to set</param>
        /// <param name="value">The value to set</param>
        void SetValue(string key, string value);
        
        /// <summary>
        /// Delete a key from the storage
        /// </summary>
        /// <param name="key">The key to delete</param>
        void DeleteKey(string key);

        /// <summary>
        /// Explicitly trigger save to persist the data to the storage
        /// </summary>
        void Save();
    }

    /// <summary>
    /// PlayerPrefsPersistentService is the default implementation of IPersistentService
    /// It leverages Unity Player preferences to persist the session data
    /// </summary>
    public class PlayerPrefsPersistentService : IPersistentService
    {
        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public string ReadValue(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }

        public void SetValue(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
    }
}
