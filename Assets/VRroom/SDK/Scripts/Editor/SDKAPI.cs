using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor;
using VRroom.Base;
using VRroom.Base.Networking;

namespace VRroom.SDK.Networking {
	[PublicAPI]
	public class SDKAPI : BaseAPI<SDKAPI> {
		public static async Task<Response> CreateContent(string name, string description, ContentType contentType, ContentWarningTags contentWarningTags) {
			return await SendRequest(CreateRequest($"{BaseUrl}/content/create", "POST", new { name, description, contentType, contentWarningTags }));
		}

		public static async Task<Response> UpdateContent(string contentId, string name = null, string description = null, ContentWarningTags? contentWarningTags = null) {
			return await SendRequest(CreateRequest($"{BaseUrl}/content/updateThumbnail", "PUT", new { contentId, name, description, contentWarningTags }));
		}

		public static async Task<Response> UpdateThumbnail(string contentId, byte[] fileData) {
			string uploadUrl = (await SendRequest(CreateRequest($"{BaseUrl}/content/update", "PUT", new { contentId }))).Result;
			
			using HttpClient client = new();
			await client.PutAsync(uploadUrl, new ByteArrayContent(fileData));
			
			return new() { Success = true };
		}

		public static async Task<Response> UpdateBundle(string contentId, string filePath) {
			using Aes aes = Aes.Create();
			
			string outputPath = Path.GetTempFileName();
			await using FileStream outputStream = File.OpenWrite(outputPath);
			using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			await outputStream.WriteAsync(aes.IV);
			
			await using (CryptoStream cryptoStream = new(outputStream, encryptor, CryptoStreamMode.Write)) {
				await using FileStream inputStream = new(filePath, FileMode.Open);
				byte[] buffer = new byte[4096];
				int bytesRead;
				while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
					await cryptoStream.WriteAsync(buffer, 0, bytesRead);
				}
			}
			
			string decryptionKey = Convert.ToBase64String(aes.Key);
			string uploadUrl = (await SendRequest(CreateRequest($"{BaseUrl}/content/updateBundle", "PUT", new { contentId, decryptionKey }))).Result;

			// unity web request just does not work for this for some reason. always give a 400 bad request
			using HttpClient client = new();
			await using FileStream fileStream = File.OpenRead(outputPath);
			await client.PutAsync(uploadUrl, new StreamContent(fileStream));
			
			return new() { Success = true };
		}
		
		[InitializeOnLoadMethod]
		private static void Init() {
			SetProfileName("SDK");
			LoadAuthToken();
		}
	}
}