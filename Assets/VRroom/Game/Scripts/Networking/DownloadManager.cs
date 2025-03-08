using System;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http;
using System.Threading.Tasks;
using Console = VRroom.Base.Debugging.Console;

namespace VRroom.Game.Networking.ContentManagement {
	public static class DownloadManager {
		private const string BaseURL = "https://vrroom.b-cdn.net";
		private static readonly HttpClient Client = new();
		public static long MaxBytesPerSecond;

		public static async Task Download(ContentRequest request, PipeWriter writer) {
			string url = $"{BaseURL}/content/{request.ContentId}/{request.VersionId}";
			
			try {
				using HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
				response.EnsureSuccessStatusCode();
				await using Stream stream = await response.Content.ReadAsStreamAsync();
				_ = await stream.ReadAsync(request.IV);
				
				while (true) {
					Memory<byte> memory = writer.GetMemory(4096);
					int bytesRead = await stream.ReadAsync(memory);
					if (bytesRead == 0) break;
					writer.Advance(bytesRead);
					
					if (MaxBytesPerSecond > 0) {
						double delayMs = bytesRead * 1000.0 / MaxBytesPerSecond;
						if (delayMs > 1) await Task.Delay((int)delayMs);
					}
				
					FlushResult result = await writer.FlushAsync();
					if (result.IsCompleted) break;
				}
			} catch (Exception e) {
				Console.Error($"Failed to download {url}\n{e}");
			}

			await writer.CompleteAsync();
		}
	}
}