using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Console = VRroom.Base.Debugging.Console;

namespace VRroom.Game.Networking {
	[PublicAPI]
	public static class DownloadManager {
		private const string BaseURL = "https://api.koneko.cat";
		private static readonly List<DownloadRequest> Requests = new();
		private static readonly HttpClient Client = new();
		private static int _concurrentDownloads;
		
		public static int MaxConcurrentDownloads = int.MaxValue;
		public static long MaxBytesPerSecond = long.MaxValue;

		public static void QueueDownload(DownloadRequest request) {
			Requests.Add(request);
			TryStartNextDownload();
		}

		private static void TryStartNextDownload() {
			if (_concurrentDownloads >= MaxConcurrentDownloads || Requests.Count == 0) return;
			
			DownloadRequest highest = Requests[0];
			foreach (DownloadRequest request in Requests) {
				if (highest.Priority < request.Priority) highest = request;
			}

			_concurrentDownloads++;
			Requests.Remove(highest);
			Download(highest);
		}

		private static async void Download(DownloadRequest request) {
			string path = Path.Combine(Application.persistentDataPath, $"{request.Type}s", request.Guid);
			string url = $"{BaseURL}/content/{ItemTypes.GetItemPrefix(request.Type)}{request.Guid}";
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			
			try {
				using HttpResponseMessage response = await Client.GetAsync($"{url}/download", HttpCompletionOption.ResponseHeadersRead);
				await using Stream stream = await response.Content.ReadAsStreamAsync();
				await using FileStream fileStream = new(path, FileMode.OpenOrCreate, FileAccess.Write);
				
				long totalBytes = response.Content.Headers.ContentLength ?? -1;
				byte[] buffer = new byte[16384];
				long totalBytesRead = 0;
				int bytesRead;

				while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
					await fileStream.WriteAsync(buffer, 0, bytesRead);
                
					if (MaxBytesPerSecond > 0) {
						double delayMs = bytesRead * 1000.0 / MaxBytesPerSecond;
						if (delayMs > 1) await Task.Delay((int)delayMs);
					}
					totalBytesRead += bytesRead;

					if (totalBytes > 0 && request.OnProgress != null) {
						request.OnProgress.Invoke((float)totalBytesRead / totalBytes);
					}
				}
			} catch (Exception e) {
				Console.Error($"Failed to download {url}\n{e}");
				request.OnFailed?.Invoke();
			} finally {
				_concurrentDownloads--;
				TryStartNextDownload();
			}
		}
	}

	[PublicAPI]
	public class DownloadRequest {
		public ItemType Type;
		public string Guid;
		public int Priority;

		public Action<float> OnProgress;
		public Action OnComplete;
		public Action OnFailed;
	}
}