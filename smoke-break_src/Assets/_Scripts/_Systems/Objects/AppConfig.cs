using System;
using System.IO;
using UnityEngine;

namespace _Scripts._Systems.Objects
{
    [Serializable]
    public class AppConfig
    {
        public string FUJIMOTO_API_URL;
        public string FUJIMOTO_API_KEY;

        private static AppConfig _instance;

        public static AppConfig Load()
        {
            if (_instance != null) return _instance;

            var path = Path.Combine(Application.streamingAssetsPath, "config.json");

            if (!File.Exists(path))
            {
                Debug.LogWarning("[AppConfig] config.json not found in StreamingAssets. Using defaults.");
                _instance = new AppConfig { FUJIMOTO_API_URL = "", FUJIMOTO_API_KEY = "" };
                return _instance;
            }

            var json = File.ReadAllText(path);
            _instance = JsonUtility.FromJson<AppConfig>(json);
            Debug.Log($"[AppConfig] Loaded API base URL: {_instance.FUJIMOTO_API_URL}");

            return _instance;
        }
    }
}
