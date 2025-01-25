using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRroom.SDK {
	public static class BuildAssetBundle {
		[MenuItem("Tools/Build Active Scene")]
		private static void BuildScene() {
			Directory.CreateDirectory(Path.Combine(Application.dataPath, "Temp"));
			
			Scene scene = SceneManager.GetActiveScene();
			string newPath = Path.Combine("Assets\\Temp", Path.GetFileName(scene.path));
			AssetDatabase.CopyAsset(scene.path, newPath);
			
			BuildAsset(newPath);
		}
		
		[MenuItem("Tools/Build Selected Object")]
		private static void BuildPrefab() {
			Directory.CreateDirectory(Path.Combine(Application.dataPath, "Temp"));
			
			GameObject gameObject = Selection.activeGameObject;
			GameObject objectCopy = Object.Instantiate(gameObject);
			
			if (PrefabUtility.IsPartOfNonAssetPrefabInstance(objectCopy) && PrefabUtility.IsOutermostPrefabInstanceRoot(objectCopy))
				PrefabUtility.UnpackPrefabInstance(objectCopy, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			
			if (objectCopy.CompareTag("EditorOnly")) objectCopy.tag = "Untagged";
			
			string newPath = Path.Combine("Assets\\Temp", $"{Path.GetFileName(objectCopy.name)}.prefab");
			PrefabUtility.SaveAsPrefabAsset(objectCopy, newPath);
			BuildAsset(newPath);
			Object.Destroy(objectCopy);
		}

		private static void BuildAsset(string assetPath) {
			AssetBundleBuild[] buildMap = {
				new() {
					assetBundleName = "content",
					assetNames = new[] { assetPath }
				}
			};

			BuildPipeline.BuildAssetBundles(
				Application.dataPath, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64
			);

			AssetDatabase.DeleteAsset(assetPath);

			string bundlePath = Path.Combine(Application.dataPath, "content");
			
		}
	}
}