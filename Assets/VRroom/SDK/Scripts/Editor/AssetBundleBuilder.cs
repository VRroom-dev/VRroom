using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRroom.Base;

namespace VRroom.SDK.Editor {
	public abstract class AssetBundleBuilder {
		public static readonly string TempPath = Path.Combine(Application.dataPath, "Temp");
		public abstract string AssetPath { get; set; }
		
		public abstract ContentDescriptor CopyAsset(ContentDescriptor descriptor);
		public abstract void Cleanup();

		public static string Build(ContentDescriptor descriptor) {
			Directory.CreateDirectory(TempPath);
			
			(AssetBundleBuilder builder, ContentType type) = descriptor switch {
				AvatarDescriptor => ((AssetBundleBuilder)new PrefabBundleBuilder(), ContentType.Avatar),
				PropDescriptor => ((AssetBundleBuilder)new PrefabBundleBuilder(), ContentType.Prop),
				WorldDescriptor => ((AssetBundleBuilder)new SceneBundleBuilder(), ContentType.World),
				GameModeDescriptor => ((AssetBundleBuilder)new SceneBundleBuilder(), ContentType.GameMode),
				_ => ((AssetBundleBuilder)new PrefabBundleBuilder(), ContentType.Avatar),
			};
			
			builder.CopyAsset(descriptor);
			AssetDatabase.Refresh();

			List<VRroomBuildPreprocessor> preprocessors = GetAllPreprocessors();
			preprocessors.ForEach(p => p.OnPreprocess(descriptor, type));
			
			AssetBundleBuild[] buildMap = {
				new() {
					assetBundleName = descriptor.guid,
					assetNames = new[] { builder.AssetPath }
				}
			};

			BuildPipeline.BuildAssetBundles(TempPath, buildMap, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
			string bundlePath = Path.Combine(TempPath, descriptor.guid);
			
			preprocessors.ForEach(p => p.OnCleanup());
			
			builder.Cleanup();
			return bundlePath;
		}
		
		private static List<VRroomBuildPreprocessor> GetAllPreprocessors() {
			List<VRroomBuildPreprocessor> preprocessors = (
				from type in TypeCache.GetTypesDerivedFrom<VRroomBuildPreprocessor>()
				where !type.IsAbstract
				let instance = TryCreatePreprocessor(type)
				where instance != null
				orderby instance.DefaultPriority
				select instance
			).ToList();
			
			List<VRroomBuildPreprocessor> sorted = new();
			List<VRroomBuildPreprocessor> remaining = new(preprocessors);

			while (remaining.Count > 0) {
				VRroomBuildPreprocessor next = remaining.FirstOrDefault(p => remaining.All(other => !MustRunBefore(other, p)));

				if (next == null) {
					Debug.LogError($"Excluding {remaining.Count} preprocessors with unresolvable dependencies");
					break;
				}
				
				sorted.Add(next);
				remaining.Remove(next);
			}

			return sorted;
			VRroomBuildPreprocessor TryCreatePreprocessor(Type type) {
				try {
					return (VRroomBuildPreprocessor)Activator.CreateInstance(type);
				} catch (Exception e) {
					Debug.LogWarning($"Skipped preprocessor {type.Name}: {e.Message}");
					return null;
				}
			}
			
			bool MustRunBefore(VRroomBuildPreprocessor a, VRroomBuildPreprocessor b) {
				PreprocessOrder aToB = a.RunsBeforeOrAfter(b.GetType().FullName);
				PreprocessOrder bToA = b.RunsBeforeOrAfter(a.GetType().FullName);
				return aToB == PreprocessOrder.Before ||
					   bToA == PreprocessOrder.After;
			}
		}
	}
}