using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace VRroom.Game.Networking.ContentManagement {
	public class ContentRequest : IDisposable, IAsyncDisposable {
		internal MemoryStream Output;
		internal volatile byte[] IV;
		internal readonly string ContentId;
		internal readonly string VersionId;

		public ContentRequest(string contentId, string versionId) {
			ContentId = contentId;
			VersionId = versionId;
		}

		public AssetBundle Start() {
			Output = new();
			IV = new byte[16];
			_ = CacheManager.GetContent(this);
			return AssetBundle.LoadFromStream(Output);
		}

		public void Dispose() {
			Output?.Dispose();
		}

		public async ValueTask DisposeAsync() {
			if (Output != null) await Output.DisposeAsync();
		}
	}
}