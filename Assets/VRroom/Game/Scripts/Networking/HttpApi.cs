using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace VRroom.Game.Networking {
	[PublicAPI]
	public static class HttpApi {
		private const string BaseUrl = "https://api.koneko.cat";
		private static string _authToken;

		public static void SetAuthToken(string token) => _authToken = token;

		#region Auth
	
		public static async Task<Response> CreateAccount(string handle, string email, string password) {
			object requestData = new { handle, email, password };
        
			string json = JsonConvert.SerializeObject(requestData);
			UnityWebRequest request = new($"{BaseUrl}/account", "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
        
			return await SendRequest(request);
		}

		public static async Task<Response> Login(string username, string password) {
			object requestData = new { username, password, deviceInfo = SystemInfo.deviceModel };
        
			string json = JsonConvert.SerializeObject(requestData);
			UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/auth/login", "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
        
			Response response = await SendRequest(request);
			if (!response.Success) return response;
		
			JObject result = response.Result as JObject;
			_authToken = result?["authToken"]?.ToString();
			return response;
		}

		public static async Task<Response> GetGameToken() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/auth/game-token");
			return await SendRequest(request);
		}

		public static async Task<Response> GetJoinToken() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/auth/join-token");
			return await SendRequest(request);
		}
	
		#endregion Auth

		#region Account
	
		public static async Task<Response> GetNotifications() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/account/notifications");
			return await SendRequest(request);
		}

		public static async Task<Response> GetFriendRequests() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/account/friend-requests");
			return await SendRequest(request);
		}

		public static async Task<Response> GetFriendsList() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/account/friends");
			return await SendRequest(request);
		}
	
		#endregion Account

		#region Users
	
		public static async Task<Response> GetUser(string userId) {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/user/{userId}");
			return await SendRequest(request);
		}

		public static async Task<Response> GetUsers(List<string> userIds) {
			object requestData = new { userIds };
			string json = JsonConvert.SerializeObject(requestData);
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/users");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			return await SendRequest(request);
		}

		public static async Task<Response> SendFriendRequest(string userId) {
			UnityWebRequest request = UnityWebRequest.Post($"{BaseUrl}/user/{userId}/friend", "", "");
			return await SendRequest(request);
		}

		public static async Task<Response> RemoveFriend(string userId) {
			UnityWebRequest request = UnityWebRequest.Delete($"{BaseUrl}/user/{userId}/friend");
			return await SendRequest(request);
		}

		public static async Task<Response> BlockUser(string userId) {
			UnityWebRequest request = UnityWebRequest.Post($"{BaseUrl}/user/{userId}/block", "", "");
			return await SendRequest(request);
		}

		#endregion Users

		#region Content

		public static async Task<Response> GetMyContent() {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/content");
			return await SendRequest(request);
		}

		public static async Task<Response> GetContentInfo(string contentId) {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/content/{contentId}");
			return await SendRequest(request);
		}

		public static async Task<Response> ShareContent(string contentId) {
			UnityWebRequest request = UnityWebRequest.Put($"{BaseUrl}/content/{contentId}/share", "");
			return await SendRequest(request);
		}

		public static async Task<Response> DownloadContent(string contentId) {
			UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/content/{contentId}/download");
			return await SendRequest(request);
		}

		#endregion Content
	
		private static async Task<Response> SendRequest(UnityWebRequest request) {
			if (!string.IsNullOrEmpty(_authToken))
				request.SetRequestHeader("Authorization", _authToken);
        
			try {
				await request.SendWebRequest();
            
				if (request.result != UnityWebRequest.Result.Success)
					return new Response { Success = false, Result = request.error };

				string responseText = request.downloadHandler.text;
				object responseJson = JObject.Parse(responseText);
            
				return new Response { 
					Success = true, 
					Result = responseJson 
				};
			}
			catch (System.Exception e) {
				return new Response { Success = false, Result = e.Message };
			}
		}
	}

	public struct Response {
		public bool Success;
		public object Result;
	}
}