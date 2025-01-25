using UnityEngine;
using Wasmtime;

namespace VRroom.Base.Scripting.UnityEngine {
	public static class ComponentBindings {
		public static void BindMethods(Linker linker) {
			linker.DefineFunction("unity", "component_tag_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Component accessed, out StoreData data);
				
				string ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.tag;
				});

				BindingHelpers.WriteString(data, ret);
			});
			
			linker.DefineFunction("unity", "component_tag_set", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out Component accessed, out StoreData data);
				
				string str = BindingHelpers.ReadString(data);
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.tag = str;
				});
			});
			
			linker.DefineFunction("unity", "component_transform_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Component accessed, out GameObject root);
				
				Transform ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.transform;
				});
				
				return BindingHelpers.ReturnTrickled(root, accessed, ret);
			});
			
			linker.DefineFunction("unity", "component_gameObject_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Component accessed, out GameObject root);
				
				GameObject ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.gameObject;
				});
				
				return BindingHelpers.ReturnTrickled(root, accessed, ret);
			});
		}
	}
}