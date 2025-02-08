using System.IO;
using UnityEditor;
using UnityEngine;
using VRroom.Base;

namespace VRroom.SDK.Editor {
	public class PrefabBundleBuilder : AssetBundleBuilder {
		public override string AssetPath { get; set; }
		private GameObject _objectCopy;

		public override ContentDescriptor CopyAsset(ContentDescriptor descriptor) {
			_objectCopy = Object.Instantiate(descriptor.gameObject);
			
			if (PrefabUtility.IsPartOfNonAssetPrefabInstance(_objectCopy) && PrefabUtility.IsOutermostPrefabInstanceRoot(_objectCopy))
				PrefabUtility.UnpackPrefabInstance(_objectCopy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			
			if (_objectCopy.CompareTag("EditorOnly")) _objectCopy.tag = "Untagged";
			
			AssetPath = Path.Combine("Assets", $"{Path.GetFileName(_objectCopy.name)}.prefab");
			PrefabUtility.SaveAsPrefabAsset(_objectCopy, AssetPath);

			return _objectCopy.GetComponent<ContentDescriptor>();
		}

		public override void Cleanup() {
			Object.DestroyImmediate(_objectCopy);
			AssetDatabase.DeleteAsset(AssetPath);
		}
	}
}