using UnityEngine;

namespace VRroom.Base {
	public class ContentDescriptor : MonoBehaviour {
		public string guid;
#if UNITY_EDITOR
		public new string name;
		public string description;
		public string thumbnailPath;
		public bool explicitTag;
		public bool goreTag;
#endif
	}
}