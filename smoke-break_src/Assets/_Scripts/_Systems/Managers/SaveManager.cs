using System;
using System.IO;
using _Scripts._Gameplay._Player;
using _Scripts._Systems.Objects;
using _Scripts._Systems.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts._Systems.Managers
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

        public static async void SaveGame(PlayerProfiler player, PlayerHealth health, PistolProfiler pistol)
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var playerId = GetOrCreatePlayerId();
            _timestamp = DateTime.UtcNow.ToString("o").Replace(':', '-');
            var savePath = Path.Combine(SaveDirectory, $"savegame_{playerId}.json");

            // Create and save the data
            var data = new SaveData
            {
                player_id = playerId,
                saved_at = _timestamp,
                playerX = player.transform.position.x,
                playerY = player.transform.position.y,
                playerZ = player.transform.position.z,
                playerHealth = health.GetCurrentHealth(),
                clipAmmo = pistol.GetCurrentClip(),
                storedAmmo = pistol.GetStoredAmmo(),
                savedLevel = SceneManager.GetActiveScene().name
            };

            await File.WriteAllTextAsync(savePath, JsonUtility.ToJson(data, true));

            // Save online data
            var success = await ApiService.UploadSaveAsync(data);
            Debug.Log(success ? "[SaveManager] Cloud save uploaded." : "[SaveManager] Cloud save failed.");
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
        }

        #endregion

        #region Reset System

        public static async void ResetGame()
        {
            if (Directory.Exists(SaveDirectory) && PlayerPrefs.HasKey(PlayerIdKey))
            {
                var id = PlayerPrefs.GetString(PlayerIdKey);
                await ApiService.DeleteSaveAsync(id);

                Directory.Delete(SaveDirectory, true);
                PlayerPrefs.DeleteKey(PlayerIdKey);
                Debug.Log("All Save Data Reset!");
            }
        }

        #endregion
    }
}