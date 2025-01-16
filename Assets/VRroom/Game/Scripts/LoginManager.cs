using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRroom.Base.Debugging;
using VRroom.Game.Networking;
using Console = VRroom.Base.Debugging.Console;

namespace VRroom.Game {
	public static class LoginManager {
		private static readonly string TokenFilePath = Path.Combine(Application.persistentDataPath, "auth_token.dat");
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		public static void Init() {
			Console.AddCommand(new(CreateAccount, "CreateAccount", "Username, Email, Password"));
			Console.AddCommand(new(Login, "Login", "Username, Password"));
			LoadAuthToken();
		}

		private static async Task<string> CreateAccount(string[] args) {
			if (args.Length != 3) throw new CommandException("Invalid Number of Arguments");
			Response response = await HttpApi.CreateAccount(args[0], args[1], args[2]);
			return response.Success ? "Successfully created account" : $"Failed to Create Account: {((JObject)response.Result)["error"]}";
		}

		private static async Task<string> Login(string[] args) {
			if (args.Length != 2) throw new CommandException("Invalid Number of Arguments");
			Response response = await HttpApi.Login(args[0], args[1]);
			if (!response.Success) return $"Failed to Login: {response.Result}";
			JObject result = response.Result as JObject;
			string authToken = result?["authToken"]?.ToString();
			if (!string.IsNullOrEmpty(authToken)) SaveAuthToken(authToken);
			return $"Successfully Logged in as {args[0]}";
		}

		private static void LoadAuthToken() {
			try {
				if (!File.Exists(TokenFilePath)) return;
				string token = File.ReadAllText(TokenFilePath);
				if (!string.IsNullOrEmpty(token)) HttpApi.SetAuthToken(token);
				Console.Log("Successfully Logged in");
			}
			catch (Exception e) {
				Console.Error($"Failed to load auth token: {e.Message}");
			}
		}

		private static void SaveAuthToken(string token) {
			try {
				File.WriteAllText(TokenFilePath, token);
			}
			catch (Exception e) {
				Console.Error($"Failed to save auth token: {e.Message}");
			}
		}
	}
}