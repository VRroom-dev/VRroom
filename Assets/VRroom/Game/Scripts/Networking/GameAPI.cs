using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using VRroom.Base.Debugging;
using VRroom.Base.Networking;

namespace VRroom.Game.Networking {
	[PublicAPI]
	public class GameAPI : BaseAPI<GameAPI> {
		//public static async Task<Response> GetGameToken() {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/auth/game-token"));
		//}

		//public static async Task<Response> GetJoinToken() {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/auth/join-token"));
		//}

		//public static async Task<Response> GetNotifications() {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/account/notifications"));
		//}

		//public static async Task<Response> GetFriendRequests() {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/account/friend-requests"));
		//}

		//public static async Task<Response> GetFriendsList() {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/account/friends"));
		//}

		//public static async Task<Response> SendFriendRequest(string userId) {
		//	return await SendRequest(UnityWebRequest.Post($"{BaseUrl}/user/{userId}/friend", "", ""));
		//}

		//public static async Task<Response> RemoveFriend(string userId) {
		//	return await SendRequest(UnityWebRequest.Delete($"{BaseUrl}/user/{userId}/friend"));
		//}

		//public static async Task<Response> BlockUser(string userId) {
		//	return await SendRequest(UnityWebRequest.Post($"{BaseUrl}/user/{userId}/block", "", ""));
		//}

		//public static async Task<Response> ShareContent(string contentId) {
		//	return await SendRequest(UnityWebRequest.Put($"{BaseUrl}/content/{contentId}/share", ""));
		//}

		//public static async Task<Response> DownloadContent(string contentId, string bundleId) {
		//	return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content/{contentId}/download"));
		//}

		public static async Task<Response> GetContentKey(string contentId) {
			return await SendRequest(UnityWebRequest.Get($"{BaseUrl}/content/{contentId}/key"));
		}
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Init() {
			Console.AddCommand(new(CreateAccount, "CreateAccount", "Username, Email, Password"));
			Console.AddCommand(new(Login, "Login", "Username, Password"));
			SetProfileName("Game");
			LoadAuthToken();
		}

		private static async Task<string> Login(string[] args) {
			if (args.Length != 2) throw new CommandException("Invalid Number of Arguments");
			Response response = await Login(args[0], args[1]);
			return response.Success ? $"Successfully Logged in as {args[0]}" : $"Failed to Login: {response.Result}";
		}

		private static async Task<string> CreateAccount(string[] args) {
			if (args.Length != 3) throw new CommandException("Invalid Number of Arguments");
			Response response = await CreateAccount(args[0], args[1], args[2]);
			return response.Success ? "Successfully created account" : $"Failed to Create Account: {response.Result}";
		}
	}
}