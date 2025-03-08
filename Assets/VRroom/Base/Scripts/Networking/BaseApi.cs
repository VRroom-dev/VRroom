using System;
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
        protected const string BaseUrl = "https://api.koneko.cat/v1";
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
            return await SendRequest(CreateRequest($"{BaseUrl}/auth/register", "POST", new { handle, email, password }));
        }

        public static async Task<Response> Login(string identifier, string password) {
            Response response = await SendRequest(CreateRequest($"{BaseUrl}/auth/login", "POST", new { identifier, password }));
            if (!response.Success) return response;
            
            JObject responseJson = JObject.Parse(response.Result);
            _authToken = responseJson["token"]!.ToString();
            SaveAuthToken();
            return response;
        }

        public static async Task<Response> GetUser(string userId) {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/profile/{userId}"));
        }

        //public static async Task<Response> GetUsers(List<string> userIds) {
        //    return await SendRequest(CreateRequest($"{BaseUrl}/users", "GET", new { userIds }));
        //}

        public static async Task<Response> GetMyContent() {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content/mine"));
        }

        public static async Task<Response> GetContentInfo(string contentId) {
            return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content/{contentId}"));
        }
        
        protected static UnityWebRequest CreateRequest(string url, string method, object data) {
            string json = JsonConvert.SerializeObject(data);
            UnityWebRequest request = new(url, method) {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }

        protected static async Task<Response> SendRequest(UnityWebRequest request) {
            if (!string.IsNullOrEmpty(_authToken))
                request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

            try {
                await request.SendWebRequest();

                return new Response {
                    Success = request.result == UnityWebRequest.Result.Success,
                    Result = request.error ?? request.downloadHandler.text
                };
            }
            catch (Exception e) {
                Debug.LogException(e);
                return new Response { Success = false, Result = e.Message };
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
        public string Result;
    }
}