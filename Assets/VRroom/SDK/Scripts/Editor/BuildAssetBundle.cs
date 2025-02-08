using UnityEditor;
using UnityEngine;
using VRroom.Base;
using Object = UnityEngine.Object;

namespace VRroom.SDK.Editor {
	public static class BuildAssetBundle {
		[MenuItem("Tools/Build Active Scene")]
		private static void BuildScene() {
			ContentDescriptor descriptor = Object.FindFirstObjectByType<ContentDescriptor>();
			if (descriptor == null) {
				Debug.LogError("Descriptor not found in scene");
				return;
			}
			string bundlePath = AssetBundleBuilder.Build(descriptor);
			
			Debug.Log($"Bundle built at \"{bundlePath}\"");
		}
		
		[MenuItem("Tools/Build Selected Object")]
		private static void BuildPrefab() {
			ContentDescriptor descriptor = Selection.activeGameObject.GetComponent<ContentDescriptor>();
			if (descriptor == null) {
				Debug.LogError("Descriptor not found on object");
				return;
			}
			string bundlePath = AssetBundleBuilder.Build(descriptor);
			
			Debug.Log($"Bundle built at \"{bundlePath}\"");
		}
	}
}