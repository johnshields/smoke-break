using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.enums;
using _Scripts.Objects;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Managers.Objectives
{
    public class PodRepairSystem : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [SerializeField] private float repairDuration = 3.0f;
        [SerializeField] private List<PodPartType> requiredParts;

        private bool _isRepairing;
        private int _partsRepaired;
        private string _savePath;

        private PlayerProfiler _playerProfiler;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistolProfiler;

        #region Initialization

        private void Start()
        {
            _playerProfiler = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _pistolProfiler = FindFirstObjectByType<PistolProfiler>();

            _savePath = SaveManager.GetSaveFilePath();
        }

        #endregion

        #region Trigger Logic

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PlayerTag) && !_isRepairing)
            {
                var nextPart = GetNextRequiredPart();
                if (nextPart.HasValue)
                {
                    Debug.Log($"🔧 Player has {nextPart}! Starting repair...");
                    StartRepairProcess(nextPart.Value);
                }
                else
                {
                    Debug.Log("⚠ Player is missing required parts!");
                }
            }
        }

        private PodPartType? GetNextRequiredPart()
        {
            foreach (var part in requiredParts)
            {
                if (ComponentTrigger.Inventory.Exists(item => item.name == part.ToString()))
                {
                    return part; // Return the first part the player has
                }
            }

            return null; // No parts available
        }

        #endregion

        #region Repair Process

        private void StartRepairProcess(PodPartType part)
        {
            _isRepairing = true;
            var partName = part.ToString();
            ObjectiveManager.Instance.ShowObjective($"Repairing with {partName}...");

            StartCoroutine(RepairRoutine(part));
        }

        private IEnumerator RepairRoutine(PodPartType part)
        {
            yield return new WaitForSeconds(repairDuration);

            var partToUse = ComponentTrigger.Inventory.Find(item => item.name == part.ToString());
            if (partToUse != null)
            {
                ComponentTrigger.Inventory.Remove(partToUse);
                Destroy(partToUse);
                _partsRepaired++;
                Debug.Log($"{part} repaired! {_partsRepaired}/{requiredParts.Count} complete.");
            }

            CompleteRepair();
        }

        private void CompleteRepair()
        {
            _isRepairing = false;

            // Show the "Repair Complete!" message on the UI
            ObjectiveManager.Instance.ShowObjective("Repair Complete!");

            // Wait for a brief moment to show the "Repair Complete!" message
            StartCoroutine(ShowRepairCompleteAndMoveToNext());
        }

        private IEnumerator ShowRepairCompleteAndMoveToNext()
        {
            // Allow some time for the "Repair Complete!" message to display
            yield return new WaitForSeconds(2.0f); // Wait for 2 seconds

            // Save the game as a checkpoint after the repair is complete
            SaveCheckpoint();

            MarkObjectiveAsComplete();

            SetNextObjective();

            Debug.Log("✅ Repair Complete!");
        }

        #endregion

        #region Save/Checkpoint Logic

        private void SaveCheckpoint()
        {
            if (!File.Exists(_savePath))
            {
                Debug.LogWarning($"⚠ Save file not found at {_savePath}. Creating a new save...");
                SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
            }
            else
            {
                Debug.Log("💾 Checkpoint save completed after repair!");
                SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
            }
        }

        #endregion

        #region Objective Management

        private void MarkObjectiveAsComplete()
        {
            var loadedObjectives = ObjectiveData.LoadObjectivesFromJson();
            var lastObjectiveKey = GetLastObjectiveKeyFromSaveData();

            if (string.IsNullOrEmpty(lastObjectiveKey))
            {
                Debug.LogError("❌ Last objective key is not set in save data!");
                return;
            }

            var foundObjective = false;
            Objective completedObjective = null;

            // Loop through the objectives to find the current completed objective
            foreach (var objective in loadedObjectives.objectives.Where(objective => objective.key == lastObjectiveKey))
            {
                // Mark the current objective as completed
                objective.completed = true;
                completedObjective = objective;
                foundObjective = true;
                Debug.Log($"✅ '{objective.key}' objective marked as complete.");
                break; // Exit after finding the current objective
            }

            // If the objective was found and updated, proceed to save only that completed objective
            if (foundObjective)
            {
                var updatedObjectives = new ObjectiveList
                {
                    objectives = new List<Objective> { completedObjective }
                };

                var updatedJson = JsonUtility.ToJson(updatedObjectives, true);
                File.WriteAllText(ObjectiveData.SavePath, updatedJson);

                Debug.Log(
                    $"✅ Updated objectives list saved with only the completed objective: {completedObjective.key}");
            }
            else
            {
                Debug.LogWarning(
                    $"⚠ The objective with key '{lastObjectiveKey}' was either already completed or not found.");
            }
        }

        private void SetNextObjective()
        {
            var json = File.ReadAllText("Assets/Resources/objectives.json");
            var loadedObjectives = JsonUtility.FromJson<ObjectiveList>(json);

            if (loadedObjectives?.objectives == null)
            {
                Debug.LogError("❌ Failed to load objectives from JSON.");
                return;
            }

            var lastObjectiveKey = GetLastObjectiveKeyFromSaveData();

            if (string.IsNullOrEmpty(lastObjectiveKey))
            {
                Debug.LogError("❌ Last objective key is not set in save data!");
                return;
            }

            var currentObjectiveIndex = loadedObjectives.objectives.FindIndex(o => o.key == lastObjectiveKey);

            if (currentObjectiveIndex == -1)
            {
                Debug.LogError($"❌ Objective with key '{lastObjectiveKey}' not found in JSON.");
                return;
            }

            // Find the next objective (if any) and save it as the new lastObjectiveKey
            var nextObjective = GetNextObjective(loadedObjectives, currentObjectiveIndex);
            if (nextObjective != null)
            {
                SaveNextObjectiveKey(nextObjective.key);
                Debug.Log($"✅ Next objective '{nextObjective.key}' is now active.");
            }

            SaveUpdatedObjective(nextObjective);
            ObjectiveManager.Instance.SetObjective(nextObjective?.key);
            ObjectiveManager.Instance.ShowObjective(nextObjective?.message);
        }

        private Objective GetNextObjective(ObjectiveList loadedObjectives, int currentObjectiveIndex)
        {
            // Find the next uncompleted objective
            if (currentObjectiveIndex + 1 < loadedObjectives.objectives.Count)
            {
                return loadedObjectives.objectives[currentObjectiveIndex + 1];
            }

            return null;
        }

        private void SaveUpdatedObjective(Objective currentObjective)
        {
            var updatedObjectives = new ObjectiveList
            {
                objectives = new List<Objective> { currentObjective }
            };

            // Serialize and save the updated objectives back to JSON
            var updatedJson = JsonUtility.ToJson(updatedObjectives, true);
            File.WriteAllText(ObjectiveData.SavePath, updatedJson);

            Debug.Log("✅ Updated objectives list saved.");
        }

        private void SaveNextObjectiveKey(string key)
        {
            // Load the save data from the file
            var saveDataJson = File.ReadAllText(_savePath);
            var saveData = JsonUtility.FromJson<SaveData>(saveDataJson);

            // Update the last objective key
            saveData.lastObjective = key;

            // Save the updated save data
            File.WriteAllText(_savePath, JsonUtility.ToJson(saveData, true));
            Debug.Log($"✅ Last objective key '{key}' saved in save data.");
        }

        private string GetLastObjectiveKeyFromSaveData()
        {
            // Load the save data from the file
            var saveDataJson = File.ReadAllText(_savePath);
            var saveData = JsonUtility.FromJson<SaveData>(saveDataJson);

            // Return the last objective key
            return saveData.lastObjective;
        }
    }

    #endregion
}