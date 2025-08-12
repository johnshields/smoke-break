using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using _Scripts._Systems.Objects;
using UnityEngine;

namespace _Scripts._Systems.Services
{
    public static class ApiService
    {
        private const string BaseUrl = "http://localhost:5000";
        private static readonly HttpClient Client = new();
        private static DateTime _lastOnlineCheck = DateTime.MinValue;
        private static bool _lastOnlineStatus = false;
        private static readonly TimeSpan OnlineCheckCacheDuration = TimeSpan.FromSeconds(10);

        private static async Task<bool> IsApiOnline()
        {
            if (DateTime.UtcNow - _lastOnlineCheck < OnlineCheckCacheDuration)
                return _lastOnlineStatus;

            try
            {
                using var response = await Client.GetAsync($"{BaseUrl}/");
                _lastOnlineStatus = response.IsSuccessStatusCode;
            }
            catch
            {
                _lastOnlineStatus = false;
            }

            _lastOnlineCheck = DateTime.UtcNow;

            return _lastOnlineStatus;
        }

        private static async Task<bool> SkipIfOffline(string actionName)
        {
            if (!await IsApiOnline())
            {
                Debug.LogWarning($"[ApiService] API unreachable - {actionName} skipped.");
                return true;
            }

            return false;
        }

        public static async Task<bool> UploadSaveAsync(SaveData saveData)
        {
            if (await SkipIfOffline("Upload")) return false;

            try
            {
                var json = JsonUtility.ToJson(saveData, true);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await Client.PostAsync($"{BaseUrl}/api/saves", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] UploadSaveAsync Error: {e.Message}");
                return false;
            }
        }

        public static async Task<SaveData> DownloadSaveAsync(string playerId)
        {
            if (await SkipIfOffline("Download")) return null;

            try
            {
                var response = await Client.GetAsync($"{BaseUrl}/api/saves/{playerId}");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"[ApiService] Download failed: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var wrapper = JsonUtility.FromJson<ApiResponse<SaveData>>(json);
                return wrapper?.data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] DownloadSaveAsync Error: {e.Message}");
                return null;
            }
        }

        public static async Task<bool> DeleteSaveAsync(string playerId)
        {
            if (await SkipIfOffline("Delete")) return false;

            if (!await IsApiOnline())
            {
                Debug.LogWarning("[ApiService] API unreachable — Delete skipped.");
                return false;
            }

            try
            {
                var response = await Client.DeleteAsync($"{BaseUrl}/api/saves/{playerId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiService] DeleteSaveAsync Error: {e.Message}");
                return false;
            }
        }

        [Serializable]
        private class ApiResponse<T>
        {
            public int status;
            public T data;
        }
    }
}