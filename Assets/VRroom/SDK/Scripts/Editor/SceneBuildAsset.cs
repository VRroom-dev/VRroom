using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRroom.Base;

namespace VRroom.SDK.Editor {
	public class SceneBundleBuilder : AssetBundleBuilder {
		public override string AssetPath { get; set; }
		private string[] _originalScenes;

		public override ContentDescriptor CopyAsset(ContentDescriptor descriptor) {
			_originalScenes = new string[SceneManager.sceneCount];
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				Scene scene = SceneManager.GetSceneAt(i);
				if (scene.isDirty) EditorSceneManager.SaveScene(scene);
				_originalScenes[i] = scene.path;
			}

			List<int> descriptorPath = new();
			Transform current = descriptor.transform;
			while ((current = current.parent) != null) {
				descriptorPath.Add(current.GetSiblingIndex());
			}
			descriptorPath.Reverse();
			
			Scene originalScene = descriptor.gameObject.scene;
			AssetPath = Path.Combine("Assets", $"{descriptor.guid}.unity");
			if (!AssetDatabase.CopyAsset(originalScene.path, AssetPath)) throw new Exception("Failed to copy scene asset.");
			
			Scene copiedScene = EditorSceneManager.OpenScene(AssetPath, OpenSceneMode.Single);
			SceneManager.SetActiveScene(copiedScene);

			current = copiedScene.GetRootGameObjects()[descriptorPath[0]].transform;
			for (var i = 1; i < descriptorPath.Count; i++) {
				current = current.transform.GetChild(descriptorPath[i]);
			}

			return current.GetComponent<ContentDescriptor>();
		}

		public override void Cleanup() {
			for (int i = 0; i < _originalScenes.Length; i++) {
				EditorSceneManager.OpenScene(_originalScenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
			}
			
			AssetDatabase.DeleteAsset(AssetPath);
		}
	}
}