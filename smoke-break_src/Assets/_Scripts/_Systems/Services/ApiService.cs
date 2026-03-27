using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using _Scripts._Systems.Objects;
using UnityEngine;

namespace _Scripts._Systems.Services
{
    public static class ApiService
    {
        private static string BaseUrl => AppConfig.Load().apiBaseUrl;
        private static string ApiKey => AppConfig.Load().apiKey;
        private static readonly HttpClient Client = new();
        private static DateTime _lastOnlineCheck = DateTime.MinValue;
        private static bool _lastOnlineStatus = false;
        private static readonly TimeSpan OnlineCheckCacheDuration = TimeSpan.FromSeconds(10);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void WarmUp()
        {
            var online = await IsApiOnline();
            Debug.Log(online
                ? "[ApiService] API is online."
                : "[ApiService] API unreachable - running offline.");
        }

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
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/saves")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                var response = await Client.SendAsync(request);
                
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
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/saves/{playerId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                var response = await Client.SendAsync(request);
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

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/api/saves/{playerId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                var response = await Client.SendAsync(request);
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