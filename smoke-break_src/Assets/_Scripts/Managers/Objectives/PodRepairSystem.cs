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
            var loadedObjectives = ObjectiveData.LoadObjectivesFromJson();

            // Retrieve the last objective key from SaveData
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
                // Create a new objective list with only the current completed objective
                var updatedObjectives = new ObjectiveList
                {
                    objectives = new List<Objective> { completedObjective }
                };

                // Serialize the updated objective list back to JSON
                var updatedJson = JsonUtility.ToJson(updatedObjectives, true);
                File.WriteAllText(ObjectiveData.SavePath, updatedJson);

                Debug.Log($"✅ Updated objectives list saved with only the completed objective: {completedObjective.key}");
            }
            else
            {
                Debug.LogWarning($"⚠ The objective with key '{lastObjectiveKey}' was either already completed or not found.");
            }
        }

        private string GetLastObjectiveKeyFromSaveData()
        {
            // Load the save data from the file to get the last completed objective's key
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

            // Iterate over the objectives in the loaded JSON
            foreach (var objective in loadedObjectives.objectives)
            {
                // Check if the objective is marked as completed
                if (objective.completed)
                {
                    completedRepairs++;
                }
            }

            _partsRepaired = completedRepairs; // Update parts repaired count
            Debug.Log($"🔄 Restored repair progress: {_partsRepaired}/{requiredParts.Count} parts repaired.");
        }

        #endregion
    }
}