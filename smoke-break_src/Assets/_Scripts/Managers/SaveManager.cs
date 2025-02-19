using _Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Managers
{
    public static class SaveManager
    {
        public static void SaveGame(PlayerProfiler player, PistolProfiler pistol)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);

            PlayerPrefs.SetInt("PlayerHealth", player.GetCurrentHealth());
            PlayerPrefs.SetInt("ClipAmmo", pistol.currentClipAmmo);
            PlayerPrefs.SetInt("StoredAmmo", pistol.storedAmmo);

            string currentScene = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("SavedLevel", currentScene);

            PlayerPrefs.Save();
        }

        public static void LoadGame(PlayerProfiler player, PistolProfiler pistol)
        {
            if (!PlayerPrefs.HasKey("PlayerX")) return;

            Vector3 savedPosition = new Vector3(
                PlayerPrefs.GetFloat("PlayerX"),
                PlayerPrefs.GetFloat("PlayerY"),
                PlayerPrefs.GetFloat("PlayerZ")
            );

            player.transform.position = savedPosition;
            player.SetCurrentHealth(PlayerPrefs.GetInt("PlayerHealth"));

            pistol.SetAmmo(
                PlayerPrefs.GetInt("ClipAmmo"),
                PlayerPrefs.GetInt("StoredAmmo")
            );

            string savedLevel = PlayerPrefs.GetString("SavedLevel");
            Debug.Log($"🔄 Loading Saved Level: {savedLevel}");

            Debug.Log("Game Loaded!");
        }

        public static void ResetGame()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Save Data Reset!");
        }
    }
}