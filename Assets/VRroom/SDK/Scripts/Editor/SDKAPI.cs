using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using VRroom.Base.Networking;

namespace VRroom.SDK.Networking {
	[PublicAPI]
	public class SDKAPI : BaseAPI<SDKAPI> {
		public static async Task<Response> CreateContent(string contentType) {
			return await SendRequest(CreateRequest($"{BaseUrl}/content", "POST", new { contentType }));
		}

		public static async Task<Response> UpdateContent(string contentId, byte[] fileData = null, byte[] thumbnailData = null, string name = null, string description = null, string[] contentWarningTags = null) {
			Dictionary<string, object> metadata = new();
			if (!string.IsNullOrEmpty(name)) metadata["name"] = name;
			if (!string.IsNullOrEmpty(description)) metadata["description"] = description;
			if (contentWarningTags is { Length: > 0 }) metadata["contentWarningTags"] = contentWarningTags;
			
			WWWForm form = new WWWForm();
			if (fileData is { Length: > 0 }) form.AddBinaryData("file", fileData, "content_file", "application/octet-stream");
			if (thumbnailData is { Length: > 0 }) form.AddBinaryData("thumbnail", thumbnailData, "thumbnail_image", "image/png");
			if (metadata.Count > 0) form.AddField("metadata", JsonConvert.SerializeObject(metadata));

			UnityWebRequest request = UnityWebRequest.Post($"{BaseUrl}/content/{contentId}", form);
			request.method = UnityWebRequest.kHttpVerbPUT;
			return await SendRequest(request);
		}
		
		[InitializeOnLoadMethod]
		private static void Init() {
			SetProfileName("SDK");
			LoadAuthToken();
		}
	}
}