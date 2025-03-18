using System.IO;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Player;

namespace _Scripts.Managers.Objectives
{
    public class ComponentTrigger : MonoBehaviour
    {
        #region Fields

        private const string PlayerTag = "Player";
        private string _savePath;

        private PlayerProfiler _playerProfiler;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistolProfiler;

        [SerializeField] private string objectiveKey; // The key of the objective in JSON

        // Static inventory list to track collected objects
        public static readonly List<GameObject> Inventory = new();

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Initialize references using FindFirstObjectByType
            _playerProfiler = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _pistolProfiler = FindFirstObjectByType<PistolProfiler>();

            // Get the save file path
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

        #endregion

        #region Inventory Management

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

        #endregion

        #region Objective Management

        private void UpdateObjectiveFromJson(string key)
        {
            if (string.IsNullOrEmpty(key) || ObjectiveManager.Instance == null)
            {
                Debug.LogError("❌ objectiveKey is empty or ObjectiveManager not found!");
                return;
            }

            bool isFirstSave = !File.Exists(_savePath);
            if (isFirstSave)
            {
                Debug.LogWarning($"⚠ Save file not found at {_savePath}. Creating a new checkpoint save...");
                SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
                Debug.Log("✅ Checkpoint save created.");
            }

            string json = File.ReadAllText(_savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            ObjectiveList loadedObjectives = ObjectiveData.LoadObjectivesFromJson();
            if (loadedObjectives == null || loadedObjectives.objectives == null)
            {
                Debug.LogError("❌ Failed to load objectives from JSON.");
                return;
            }

            int currentIndex = loadedObjectives.objectives.FindIndex(o => o.key == key);
            if (currentIndex > 0)
            {
                MarkPreviousObjectiveComplete(loadedObjectives, currentIndex, saveData);
            }
            else
            {
                Debug.LogWarning($"⚠ No previous objective found for {key}");
            }

            SetNextObjective(loadedObjectives);
        }

        private void MarkPreviousObjectiveComplete(ObjectiveList loadedObjectives, int currentIndex, SaveData saveData)
        {
            Objective previousObjective = loadedObjectives.objectives[currentIndex - 1];
            previousObjective.completed = true;

            string updatedJson = JsonUtility.ToJson(loadedObjectives, true);
            File.WriteAllText(ObjectiveData.SavePath, updatedJson);

            saveData.lastObjective = previousObjective.message;
            File.WriteAllText(_savePath, JsonUtility.ToJson(saveData, true));

            ObjectiveManager.Instance.SetObjective(previousObjective.message);
            ObjectiveManager.Instance.ShowObjective(previousObjective.message);

            Debug.Log($"✅ Objective Updated & Marked as Completed: {previousObjective.key}");
        }

        private void SetNextObjective(ObjectiveList loadedObjectives)
        {
            Objective nextObjective = loadedObjectives.objectives.Find(o => !o.completed);

            if (nextObjective != null)
            {
                ObjectiveManager.Instance.SetObjective(nextObjective.key);
                ObjectiveManager.Instance.ShowObjective(nextObjective.message);

                string updatedJson = JsonUtility.ToJson(loadedObjectives, true);
                File.WriteAllText(ObjectiveData.SavePath, updatedJson);

                Debug.Log($"🎯 Next Objective Set: {nextObjective.message}");
            }
            else
            {
                Debug.Log("⚠ No more objectives available.");
            }
        }

        #endregion
    }
}