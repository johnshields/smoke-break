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
        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        private const string PlayerIdKey = "PlayerId";

        #region Player ID

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

        #region Save

        public static async void SaveGame(PlayerProfiler player, PlayerHealth health, PistolProfiler pistol)
        {
            try
            {
                if (!Directory.Exists(SaveDirectory))
                    Directory.CreateDirectory(SaveDirectory);

                var data = new SaveData
                {
                    player_id = GetOrCreatePlayerId(),
                    saved_at = DateTime.UtcNow.ToString("o"),
                    playerX = player.transform.position.x,
                    playerY = player.transform.position.y,
                    playerZ = player.transform.position.z,
                    playerHealth = health.GetCurrentHealth(),
                    clipAmmo = pistol.GetCurrentClip(),
                    storedAmmo = pistol.GetStoredAmmo(),
                    savedLevel = SceneManager.GetActiveScene().name
                };

                await File.WriteAllTextAsync(GetSaveFilePath(), JsonUtility.ToJson(data, true));

                var success = await ApiService.UploadSaveAsync(data);
                Debug.Log(success ? "[SaveManager] Cloud save uploaded." : "[SaveManager] Cloud save failed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] SaveGame failed: {e.Message}");
            }
        }

        #endregion

        #region Load

        public static void LoadGame(PlayerProfiler player, PlayerHealth health, PistolProfiler pistol)
        {
            var savePath = GetSaveFilePath();
            if (!File.Exists(savePath)) return;

            try
            {
                var json = File.ReadAllText(savePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                health.SetCurrentHealth(data.playerHealth);
                pistol.SetAmmo(data.clipAmmo, data.storedAmmo);

                Debug.Log($"[SaveManager] Loading level: {data.savedLevel}");
                SceneManager.LoadScene(data.savedLevel);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] LoadGame failed: {e.Message}");
            }
        }

        #endregion

        #region Reset

        public static async void ResetGame()
        {
            try
            {
                if (!Directory.Exists(SaveDirectory) || !PlayerPrefs.HasKey(PlayerIdKey)) return;

                var id = PlayerPrefs.GetString(PlayerIdKey);
                await ApiService.DeleteSaveAsync(id);

                Directory.Delete(SaveDirectory, true);
                PlayerPrefs.DeleteKey(PlayerIdKey);
                Debug.Log("[SaveManager] All save data reset.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] ResetGame failed: {e.Message}");
            }
        }

        #endregion
    }
}
