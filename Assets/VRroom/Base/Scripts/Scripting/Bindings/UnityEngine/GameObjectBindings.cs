using UnityEngine;
using UnityEngine.SceneManagement;
using Wasmtime;

namespace VRroom.Base.Scripting.UnityEngine {
	public static class GameObjectBindings {
		public static void BindMethods(Linker linker) {
			linker.DefineFunction("unity", "gameObject_ctor", (Caller caller, int objectId) => {
				
			});
			
			linker.DefineFunction("unity", "gameObject_activeInHierarchy_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
				
				bool ret = false;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.activeInHierarchy;
				});

				return ret;
			});
			
			linker.DefineFunction("unity", "gameObject_activeSelf_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
				
				bool ret = false;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.activeSelf;
				});

				return ret;
			});
			
			linker.DefineFunction("unity", "gameObject_isStatic_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
				
				bool ret = false;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.isStatic;
				});

				return ret;
			});
			
			linker.DefineFunction("unity", "gameObject_isStatic_set", (Caller caller, int objectId, bool isStatic) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out GameObject accessed);
				
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.isStatic = isStatic;
				});
			});
			
			linker.DefineFunction("unity", "gameObject_layer_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
				
				int ret = 0;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.layer;
				});

				return ret;
			});
			
			linker.DefineFunction("unity", "gameObject_layer_set", (Caller caller, int objectId, int layer) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out GameObject accessed);
				
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.layer = layer;
				});
			});
			
			// needs way to track scenes like we track objects. this should be easy. possibly use `Scene.handle` in place of `Object.GetInstanceID()`
			//linker.DefineFunction("unity", "gameObject_scene_get", (Caller caller, int objectId) => {
			//	BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
			//	
			//	Scene ret = default;
			//	WasmManager.ExecuteMainThreadAction(() => {
			//		ret = accessed.scene;
			//	});
			//	
			//	return ret;
			//});
			
			linker.DefineFunction("unity", "gameObject_sceneCullingMask_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed);
				
				ulong ret = 0;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.sceneCullingMask;
				});

				return ret;
			});
			
			linker.DefineFunction("unity", "gameObject_tag_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out GameObject accessed, out StoreData data);
				
				string ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.tag;
				});

				BindingHelpers.WriteString(data, ret);
			});
			
			linker.DefineFunction("unity", "gameObject_tag_set", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out GameObject accessed, out StoreData data);
				
				string str = BindingHelpers.ReadString(data);
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.tag = str;
				});
			});
			
			linker.DefineFunction("unity", "gameObject_transform_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Component accessed, out GameObject root);
				
				Transform ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.transform;
				});
				
				return BindingHelpers.ReturnTrickled(root, accessed, ret);
			});
		}
	}
}