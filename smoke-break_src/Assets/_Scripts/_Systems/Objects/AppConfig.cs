using System;
using System.IO;
using UnityEngine;

namespace _Scripts._Systems.Objects
{
    [Serializable]
    public class AppConfig
    {
        public string apiBaseUrl;
        public string apiKey;

        private static AppConfig _instance;

        public static AppConfig Load()
        {
            if (_instance != null) return _instance;

            var path = Path.Combine(Application.streamingAssetsPath, "config.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning("[AppConfig] config.json not found in StreamingAssets. Using defaults.");
                _instance = new AppConfig { apiBaseUrl = "", apiKey = "" };
                return _instance;
            }

            var json = File.ReadAllText(path);
            _instance = JsonUtility.FromJson<AppConfig>(json);
            Debug.Log($"[AppConfig] Loaded API base URL: {_instance.apiBaseUrl}");

            return _instance;
        }
    }
}
