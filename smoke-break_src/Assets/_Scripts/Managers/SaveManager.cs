using System;
using System.IO;
using _Scripts.Managers.Objectives;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Managers
{
    public static class SaveManager
    {
        #region Constants & Paths

        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        private const string PlayerIdKey = "PlayerId";
        private static string _timestamp = DateTime.UtcNow.ToString("o").Replace(':', '-');

        #endregion

        #region Player ID Handling

        private static string GetOrCreatePlayerId()
        {
            if (!PlayerPrefs.HasKey(PlayerIdKey))
            {
                var newId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(PlayerIdKey, newId);
                PlayerPrefs.Save();
            }

            return PlayerPrefs.GetString(PlayerIdKey);
        }

        public static string GetSaveFilePath()
        {
            return Path.Combine(SaveDirectory, $"savegame_{GetOrCreatePlayerId()}.json");
        }

        #endregion

        #region Save System

        public static void SaveGame(PlayerProfiler player, PlayerHealth health, PistolProfiler pistol)
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var playerId = GetOrCreatePlayerId();
            _timestamp = DateTime.UtcNow.ToString("o").Replace(':', '-');
            var savePath = Path.Combine(SaveDirectory, $"savegame_{playerId}.json");

            // Retrieve last objective, ensuring no overwrite it with an empty value
            var lastObjective = ObjectiveManager.Instance != null ? ObjectiveManager.Instance.GetLastObjective() : "";
            if (File.Exists(savePath))
            {
                var existingData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
                if (string.IsNullOrEmpty(lastObjective))
                {
                    lastObjective = existingData.lastObjective; // Preserve last known objective
                }
            }

            // Create and save the data
            var data = new SaveData
            {
                playerId = playerId,
                timestamp = _timestamp,
                playerX = player.transform.position.x,
                playerY = player.transform.position.y,
                playerZ = player.transform.position.z,
                playerHealth = health.GetCurrentHealth(),
                clipAmmo = pistol.GetCurrentClip(),
                storedAmmo = pistol.GetStoredAmmo(),
                savedLevel = SceneManager.GetActiveScene().name,
                lastObjective = lastObjective
            };

            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        }

        #endregion

        #region Load System

        public static void LoadGame(PlayerProfiler player, PlayerHealth health, PistolProfiler pistol)
        {
            var playerId = GetOrCreatePlayerId();
            var savePath = Path.Combine(SaveDirectory, $"savegame_{playerId}.json");

            if (!File.Exists(savePath)) return;

            var json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            // Restore player state
            player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            health.SetCurrentHealth(data.playerHealth);
            pistol.SetAmmo(data.clipAmmo, data.storedAmmo);

            Debug.Log($"Game Loading... \n Player Object: {data}");
            SceneManager.LoadScene(data.savedLevel);

            // Restore last objective if available
            if (!string.IsNullOrEmpty(data.lastObjective) && ObjectiveManager.Instance != null)
            {
                Debug.Log($"✅ Restoring Last Objective: {data.lastObjective}");
                ObjectiveManager.Instance.RestoreLastObjective(data.lastObjective);
            }
        }

        #endregion

        #region Reset System

        public static void ResetGame()
        {
            if (Directory.Exists(SaveDirectory))
            {
                Directory.Delete(SaveDirectory, true);
                PlayerPrefs.DeleteKey(PlayerIdKey);
                Debug.Log("All Save Data Reset!");
            }
        }

        #endregion
    }
}