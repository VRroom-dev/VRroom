using System;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Console = VRroom.Base.Debugging.Console;

namespace VRroom.Game.Networking.ContentManagement {
	public static class DecryptionManager {
		public static async Task DecryptContent(ContentRequest request, PipeReader reader) {
			string base64Key = (await GameAPI.GetContentKey(request.ContentId)).Result;
			byte[] key = Convert.FromBase64String(base64Key);

			try {
				using Aes aes = Aes.Create();
				aes.Key = key;
				aes.IV = request.IV;
				
				using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
				await using CryptoStream cryptoStream = new(request.Output, decryptor, CryptoStreamMode.Write);

				while (true) {
					ReadResult result = await reader.ReadAsync();
					foreach (ReadOnlyMemory<byte> memory in result.Buffer) {
						await cryptoStream.WriteAsync(memory);
					}
					
					reader.AdvanceTo(result.Buffer.End);
					if (result.IsCompleted) break;
				}
			} catch (Exception e) {
				Console.Error($"Failed to decrypt.\n{e}");
			}
				
			await reader.CompleteAsync();
		}
	}
}