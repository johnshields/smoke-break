using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _Scripts.Managers.Objectives
{
    public static class ObjectiveData
    {
        private static readonly Dictionary<string, Objective> Objectives = new();
        private static readonly string SavePath = Application.persistentDataPath + "/objectives.json";

        public static void LoadObjectivesFromJson()
        {
            var jsonFile = Resources.Load<TextAsset>("objectives");
            if (jsonFile == null)
            {
                Debug.LogError("❌ objectives.json not found in Resources/");
                return;
            }

            var data = JsonUtility.FromJson<ObjectiveList>(jsonFile.text);
            if (data?.objectives == null)
            {
                Debug.LogError("❌ Failed to parse objectives.json");
                return;
            }

            Objectives.Clear();

            foreach (var obj in data.objectives)
            {
                if (string.IsNullOrEmpty(obj.key)) continue;
                Objectives[obj.key] = obj;
            }
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