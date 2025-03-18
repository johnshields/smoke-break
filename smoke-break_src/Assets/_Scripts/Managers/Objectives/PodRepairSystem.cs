using System.IO;
using System.Collections;
using System.Collections.Generic;
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

            RestoreRepairProgress();
        }
        #endregion

        #region Trigger Logic
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(PlayerTag) && !_isRepairing)
            {
                PodPartType? nextPart = GetNextRequiredPart();
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
            foreach (PodPartType part in requiredParts)
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
            string partName = part.ToString();
            ObjectiveManager.Instance.ShowObjective($"Repairing with {partName}...");

            StartCoroutine(RepairRoutine(part));
        }

        private IEnumerator RepairRoutine(PodPartType part)
        {
            yield return new WaitForSeconds(repairDuration);

            GameObject partToUse = ComponentTrigger.Inventory.Find(item => item.name == part.ToString());
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

            // Mark "FoundJack" as complete
            MarkObjectiveAsComplete();

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
            // Load the current objectives from the JSON
            ObjectiveList loadedObjectives = ObjectiveData.LoadObjectivesFromJson();

            // Retrieve the last objective key from SaveData
            string lastObjectiveKey = GetLastObjectiveKeyFromSaveData();

            if (string.IsNullOrEmpty(lastObjectiveKey))
            {
                Debug.LogError("❌ Last objective key is not set in save data!");
                return;
            }

            bool foundObjective = false;

            // Loop through the objectives to find the one matching the last objective key
            foreach (var objective in loadedObjectives.objectives)
            {
                if (objective.key == lastObjectiveKey && !objective.completed)
                {
                    // Mark this objective as complete
                    objective.completed = true;
                    foundObjective = true;

                    // Save the updated objective list back to JSON
                    string updatedJson = JsonUtility.ToJson(loadedObjectives, true);
                    File.WriteAllText(ObjectiveData.SavePath, updatedJson);

                    Debug.Log($"✅ '{objective.key}' objective marked as complete.");
                    break; // Exit the loop after marking the objective as complete
                }
            }

            // If the objective was not found or completed, log a warning
            if (!foundObjective)
            {
                Debug.LogWarning(
                    $"⚠ The objective with key '{lastObjectiveKey}' was either already completed or not found.");
            }
        }

        private string GetLastObjectiveKeyFromSaveData()
        {
            // Load the save data from the file
            string saveDataJson = File.ReadAllText(_savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(saveDataJson);

            // Return the last objective key
            return saveData.lastObjective;
        }
        #endregion

        #region Repair Progress Restoration
        private void RestoreRepairProgress()
        {
            int completedRepairs = 0;
            ObjectiveList loadedObjectives = ObjectiveData.LoadObjectivesFromJson();

            foreach (PodPartType part in requiredParts)
            {
                bool partFound = ComponentTrigger.Inventory.Exists(item => item.name == part.ToString());

                // Check if the objective associated with this part is completed
                Objective objective = loadedObjectives.objectives.Find(o => o.key == part.ToString());
                if (objective is { completed: true } && !partFound)
                {
                    // Restore the missing part in inventory
                    GameObject restoredPart = new GameObject(part.ToString()); // Replace with actual prefab if needed
                    restoredPart.name = part.ToString();
                    ComponentTrigger.Inventory.Add(restoredPart);
                    Debug.Log($"🔄 Restored {part} to inventory from saved progress.");
                }

                if (!partFound)
                {
                    break; // Stop at the first missing part
                }

                completedRepairs++;
            }

            _partsRepaired = completedRepairs;
            Debug.Log($"🔄 Restored repair progress: {_partsRepaired}/{requiredParts.Count} parts repaired.");
        }
        #endregion
    }
}
