using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRroom.SDK {
	public static class BuildAssetBundle {
		[MenuItem("Tools/Build Scene")]
		private static void BuildScene() {
			Scene scene = SceneManager.GetActiveScene();
			
			AssetBundleBuild[] buildMap = {
				new() {
					assetBundleName = "Content",
					assetNames = new[] { scene.path }
				}
			};

			BuildPipeline.BuildAssetBundles(
				Application.dataPath, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64
			);
		}
	}
}