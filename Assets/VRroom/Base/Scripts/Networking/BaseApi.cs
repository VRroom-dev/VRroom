using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation
namespace VRroom.Base.Networking {
	[PublicAPI]
	public abstract class BaseAPI<T> where T : BaseAPI<T> {
        protected const string BaseUrl = "https://api.koneko.cat";
        // ReSharper disable StaticMemberInGenericType
        private static string _authTokenPath;
        private static string _authToken;

        // need endpoint to test authentication in api
        public static bool IsAuthenticated() => !string.IsNullOrEmpty(_authToken);

        public static void Logout() {
            _authToken = "";
            SaveAuthToken();
        }

        public static async Task<Response> CreateAccount(string handle, string email, string password) {
            return await SendRequest(CreateRequest($"{BaseUrl}/account", "POST", new { handle, email, password }));
        }

        public static async Task<Response> Login(string username, string password) {
            object requestData = new { username, password, deviceInfo = SystemInfo.deviceModel };
            Response response = await SendRequest(CreateRequest($"{BaseUrl}/auth/login", "POST", requestData));
            if (!response.Success) return response;

            JObject result = response.Result as JObject;
            _authToken = result?["authToken"]?.ToString();
            SaveAuthToken();
            return response;
        }

        public static async Task<Response> GetUser(string userId) {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/user/{userId}"));
        }

        public static async Task<Response> GetUsers(List<string> userIds) {
            return await SendRequest(CreateRequest($"{BaseUrl}/users", "GET", new { userIds }));
        }

        public static async Task<Response> GetMyContent() {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content"));
        }

        public static async Task<Response> GetContentInfo(string contentId) {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content/{contentId}"));
        }
        
        protected static UnityWebRequest CreateRequest(string url, string method, object data) {
            string json = JsonConvert.SerializeObject(data);
            var request = new UnityWebRequest(url, method) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }

        protected static async Task<Response> SendRequest(UnityWebRequest request) {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", _authToken);

            try {
                await request.SendWebRequest();

                string responseText = request.downloadHandler.text;
                JObject responseJson = JObject.Parse(responseText);
                
                return request.result != UnityWebRequest.Result.Success ? 
                    new Response { Success = false, Error = responseJson["error"]?.ToString() } : 
                    new Response { Success = true, Result = responseJson };
            }
            catch (Exception e) {
                Debug.LogException(e);
                return new Response { Success = false, Error = e.Message };
            }
        }
        
        protected static void SetProfileName(string profileName) {
            _authTokenPath = Path.Combine(Application.persistentDataPath, $"auth_token_{profileName}.dat");
        }

        protected static void LoadAuthToken() {
            try {
                if (!File.Exists(_authTokenPath)) return;
                string token = File.ReadAllText(_authTokenPath);
                if (!string.IsNullOrEmpty(token)) _authToken = token;
            } catch (Exception e) {
                Debug.LogError($"Failed to load auth token: {e.Message}");
            }
        }

        private static void SaveAuthToken() {
            try {
                File.WriteAllText(_authTokenPath, _authToken);
            } catch (Exception e) {
                Debug.LogError($"Failed to save auth token: {e.Message}");
            }
        }
    }

    public struct Response {
        public bool Success;
        public string Error;
        public JObject Result;
    }
}