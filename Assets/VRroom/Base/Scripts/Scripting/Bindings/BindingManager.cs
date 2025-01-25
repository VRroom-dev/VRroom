using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Wasmtime;
using VRroom.Base.Scripting.UnityEngine;
using Object = UnityEngine.Object;

namespace VRroom.Base.Scripting {
	public static class BindingManager {
		private static readonly ConcurrentDictionary<(int, int), bool> AccessResults = new();
		private static readonly ConcurrentDictionary<(int, int), bool> TrickledPermissions = new();
		private static readonly ConcurrentDictionary<int, Object> InstanceToObject = new();
		private static readonly HashSet<Type> ComponentWhitelist = new();
		private static readonly HashSet<Type> ObjectWhitelist = new();
		private static readonly HashSet<Type> InternalReadonlyBlacklist = new();
		private static readonly object WhitelistLock;
		
		public static void BindMethods(Linker linker) {
			WasiStubs.DefineWasiFunctions(linker);
			DebugBindings.BindMethods(linker);
			ObjectBindings.BindMethods(linker);
			GameObjectBindings.BindMethods(linker);
			ComponentBindings.BindMethods(linker);
			
			
			linker.DefineFunction("unity", "transform_parent_set", (Caller caller, int objectId, int objectId2) => {
				GameObject root = BindingHelpers.GetData(caller).Root;
				Transform accessed = GetObjectById<Transform>(objectId);
				Transform assigned = GetObjectById<Transform>(objectId2);
				ThrowIfCantAccessObject(root, accessed, false);
				
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.parent = assigned;
				});
			});
		}
		
		public static void Cleanup() {
			List<int> invalidInstanceIds = new();
        
			foreach ((int key, Object value) in InstanceToObject) {
				if (value == null) invalidInstanceIds.Add(key);
			}
        
			foreach (int instanceId in invalidInstanceIds) {
				InstanceToObject.TryRemove(instanceId, out _);
            
				foreach ((int, int) key in AccessResults.Keys) {
					if (key.Item1 == instanceId || key.Item2 == instanceId)
						AccessResults.TryRemove(key, out _);
				}
            
				foreach ((int, int) key in TrickledPermissions.Keys) {
					if (key.Item1 == instanceId || key.Item2 == instanceId)
						TrickledPermissions.TryRemove(key, out _);
				}
			}
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TrackObject(Object obj) {
			if (obj != null) InstanceToObject.TryAdd(obj.GetInstanceID(), obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetObjectById<T>(int id) where T : Object {
			if (InstanceToObject.TryGetValue(id, out Object obj)) return obj as T;
			return null;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TricklePermissions(GameObject root, Object parent, Object child) {
			int rootId = root.GetInstanceID();
			int parentId = parent.GetInstanceID();
			int childId = child.GetInstanceID();

			(int, int) identifier1 = (rootId, parentId);
			(int, int) identifier2 = (rootId, childId);
			
			if (TrickledPermissions.TryGetValue(identifier1, out bool parentCanWrite)) {
				TrickledPermissions.TryAdd(identifier2, parentCanWrite);
				return;
			}
    
			bool canWrite = parent switch {
				GameObject go => CanModifyGameObject(root, go),
				Component comp => CanModifyGameObject(root, comp.gameObject),
				_ => false
			};
    
			TrickledPermissions.TryAdd(identifier2, canWrite);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ThrowIfCantAccessObject(GameObject root, Object accessing, bool reading) {
			(int, int) identifier = (root.GetInstanceID(), accessing.GetInstanceID());
			if (TrickledPermissions.TryGetValue(identifier, out bool hasWritePermission)) {
				if (reading || hasWritePermission) return;
				throw new InvalidOperationException("Wasm attempting to write with read-only permission");
			}
			
			if (!AccessResults.GetOrAdd(identifier, _ => {
				if (accessing is GameObject gameObject) {
					if (root == accessing) return false;
					return reading || CanModifyGameObject(root, gameObject);
				}
				
				lock (WhitelistLock) {
					Type type = accessing.GetType();
					if (InternalReadonlyBlacklist.Contains(type)) return reading;
					if (accessing is not Component component) return ObjectWhitelist.Contains(type);
					if (!ComponentWhitelist.Contains(type)) return false;
					return reading || CanModifyGameObject(root, component.gameObject);
				}
			})) throw new InvalidOperationException("Wasm attempting to write with read-only permission");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CanModifyGameObject(GameObject a, GameObject b) {
			if (IsOutsideScene(a, b)) return false;
			return IsWorld(a) || IsChildGameObject(a, b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsWorld(GameObject a) => !a.scene.isSubScene;
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsOutsideScene(GameObject a, GameObject b) => a.scene != b.scene;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsChildGameObject(GameObject parent, GameObject child) => child.transform.IsChildOf(parent.transform);
	}
}