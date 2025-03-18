using System.Collections.Generic;
using System.IO;
using _Scripts.Objects;
using UnityEngine;

namespace _Scripts.Managers.Objectives
{
    public static class ObjectiveData
    {
        public static readonly Dictionary<string, Objective> Objectives = new();
        public static readonly string SavePath = Application.persistentDataPath + "/objectives.json";

        public static ObjectiveList LoadObjectivesFromJson()
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("⚠ No saved objectives found. Loading default objectives.");
            }

            var jsonFile = Resources.Load<TextAsset>("objectives");
            if (jsonFile == null)
            {
                Debug.LogError("❌ objectives.json not found in Resources/");
                return null; // Return null if file is missing
            }

            var data = JsonUtility.FromJson<ObjectiveList>(jsonFile.text);
            if (data?.objectives == null)
            {
                Debug.LogError("❌ Failed to parse objectives.json");
                return null; // Return null if parsing failed
            }

            if (Objectives.Count == 0) // Prevent clearing if objectives already exist
            {
                Objectives.Clear();
            }

            foreach (var obj in data.objectives)
            {
                if (string.IsNullOrEmpty(obj.key)) continue;
                Objectives.TryAdd(obj.key, obj); // Avoid overwriting existing objectives
            }

            return data; // ✅ Return the parsed ObjectiveList
        }

        public static Objective GetObjective(string key)
        {
            if (Objectives.TryGetValue(key, out var objective))
            {
                return objective;
            }

            Debug.LogError($"❌ Objective '{key}' not found in objectives.json");
            return null;
        }

        public static void CompleteObjective(string key)
        {
            if (!Objectives.TryGetValue(key, out var objective)) return;
            if (objective.completed) return;

            objective.completed = true;
            SaveObjectivesToFile();
        }

        private static void SaveObjectivesToFile()
        {
            var data = new ObjectiveList { objectives = new List<Objective>(Objectives.Values) };
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
    }
}