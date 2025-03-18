using System.IO;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Player;

namespace _Scripts.Managers.Objectives
{
    public class ComponentTrigger : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private string _savePath;

        private PlayerProfiler _playerProfiler;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistolProfiler;

        [SerializeField] private string objectiveKey; // The key of the objective in JSON

        // ✅ Static inventory list to track collected objects
        private static readonly List<GameObject> Inventory = new();

        private void Start()
        {
            // ✅ Initialize references using FindFirstObjectByType
            _playerProfiler = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _pistolProfiler = FindFirstObjectByType<PistolProfiler>();

            // ✅ Get the save file path
            _savePath = SaveManager.GetSaveFilePath();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                Debug.Log($"Player triggered: {gameObject.name}");

                AddToInventory(gameObject);
                UpdateObjectiveFromJson(objectiveKey);

                // Disable the object after collecting
                gameObject.SetActive(false);
            }
        }

        private void AddToInventory(GameObject item)
        {
            if (!Inventory.Contains(item))
            {
                Inventory.Add(item);
                Debug.Log($"🛠 Added {item.name} to inventory. Total items: {Inventory.Count}");
            }
            else
            {
                Debug.Log($"⚠ {item.name} is already in inventory. Ignoring duplicate.");
            }
        }

        private void UpdateObjectiveFromJson(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("❌ objectiveKey is empty! Make sure it's assigned in the Inspector.");
                return;
            }

            if (ObjectiveManager.Instance == null)
            {
                Debug.LogError("❌ ObjectiveManager instance not found!");
                return;
            }

            // Ensure save file exists before updating objectives
            bool isFirstSave = !File.Exists(_savePath);
            if (isFirstSave)
            {
                Debug.LogWarning($"⚠ Save file not found at {_savePath}. Creating a new checkpoint save...");
                SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
                Debug.Log("✅ Checkpoint save created using SaveManager.");
            }

            // Load (or create) save file using SaveManager
            string json = File.ReadAllText(_savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // Load objectives from JSON
            ObjectiveList loadedObjectives = ObjectiveData.LoadObjectivesFromJson();
            if (loadedObjectives == null || loadedObjectives.objectives == null)
            {
                Debug.LogError("❌ Failed to load objectives from JSON.");
                return;
            }

            // ✅ Use dictionary lookup for efficiency
            if (ObjectiveData.Objectives.TryGetValue(key, out Objective obj))
            {
                Debug.Log($"✅ Objective found in JSON: {obj.message}");

                // ✅ Mark the objective as completed
                obj.completed = true;

                // ✅ Update player's last objective
                saveData.lastObjective = obj.message;

                // ✅ Save progress **only once**, instead of calling `SaveGame()` twice
                File.WriteAllText(_savePath, JsonUtility.ToJson(saveData, true));

                // ✅ Save updated objectives (to mark completion)
                string updatedJson = JsonUtility.ToJson(loadedObjectives, true);
                File.WriteAllText(ObjectiveData.SavePath, updatedJson);

                // ✅ Update the UI with the new objective
                ObjectiveManager.Instance.SetObjective(obj.message);
                ObjectiveManager.Instance.ShowObjective(obj.message);

                Debug.Log($"✅ Objective Updated & Marked as Completed: {obj.message}");
            }
            else
            {
                Debug.LogWarning($"⚠ Objective with key '{key}' not found in JSON.");
            }
        }
    }
}