using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using UnityEngine;

namespace VRroom.Game.Networking.ContentManagement {
	public static class CacheManager {
		private static readonly string CachePath = Path.Combine(Application.persistentDataPath, "ContentCache");

		private static async Task ReadCache(ContentRequest request, PipeWriter writer, string path) {
			await using FileStream fileStream = File.OpenRead(path);
			_ = await fileStream.ReadAsync(request.IV);
			
			while (true) {
				Memory<byte> memory = writer.GetMemory(4096);
				int bytesRead = await fileStream.ReadAsync(memory);
				if (bytesRead == 0) break;
				writer.Advance(bytesRead);
				
				FlushResult result = await writer.FlushAsync();
				if (result.IsCompleted) break;
			}
			
			await writer.CompleteAsync();
		}

		private static async Task WriteCache(ContentRequest request, PipeReader reader, PipeWriter writer, string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			await using FileStream fileStream = File.OpenWrite(path);

			bool wroteIv = false;
			while (true) {
				ReadResult result = await reader.ReadAsync();
				if (!wroteIv) {
					await fileStream.WriteAsync(request.IV);
					wroteIv = true;
				}
				
				foreach (ReadOnlyMemory<byte> memory in result.Buffer) {
					await fileStream.WriteAsync(memory);
					await writer.WriteAsync(memory);
				}
				
				reader.AdvanceTo(result.Buffer.End);
				FlushResult flushResult = await writer.FlushAsync();
				if (flushResult.IsCompleted) break;
				if (result.IsCompleted) break;
			}
			
			await reader.CompleteAsync();
			await writer.CompleteAsync();
		}

		public static async Task GetContent(ContentRequest request) {
			string path = Path.Combine(CachePath, request.ContentId, request.VersionId);

			if (File.Exists(path)) {
				Pipe pipe = new();
				Task readCache = ReadCache(request, pipe.Writer, path);
				Task decryptContent = DecryptionManager.DecryptContent(request, pipe.Reader);
				
				await Task.WhenAll(readCache, decryptContent);
			} else {
				Pipe pipe1 = new(), pipe2 = new();
				Task downloadContent = DownloadManager.Download(request, pipe1.Writer);
				Task writeCache = WriteCache(request, pipe1.Reader, pipe2.Writer, path);
				Task decryptContent = DecryptionManager.DecryptContent(request, pipe2.Reader);
				
				await Task.WhenAll(downloadContent, writeCache, decryptContent);
			}
		}
	}
}