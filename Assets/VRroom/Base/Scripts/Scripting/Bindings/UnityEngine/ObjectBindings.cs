using UnityEngine;
using Wasmtime;

namespace VRroom.Base.Scripting.UnityEngine {
	public static class ObjectBindings {
		public static void BindMethods(Linker linker) {
			linker.DefineFunction("unity", "object_name_get", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Object accessed, out StoreData data);
				
				string ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.name;
				});
				
				BindingHelpers.WriteString(data, ret);
			});
			
			linker.DefineFunction("unity", "object_name_set", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out Object accessed, out StoreData data);
				
				string str = BindingHelpers.ReadString(data);
				WasmManager.ExecuteMainThreadAction(() => {
					accessed.name = str;
				});
			});
			
			linker.DefineFunction("unity", "object_toString", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, true, out Object accessed, out StoreData data);
				
				string ret = null;
				WasmManager.ExecuteMainThreadAction(() => {
					ret = accessed.ToString();
				});
				
				BindingHelpers.WriteString(data, ret);
			});
			
			linker.DefineFunction("unity", "object_destroy", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out Object accessed);
				
				WasmManager.ExecuteMainThreadAction(() => {
					Object.Destroy(accessed);
				});
			});
			
			linker.DefineFunction("unity", "object_instantiate", (Caller caller, int objectId) => {
				BindingHelpers.GetAccessed(caller, objectId, false, out Object accessed);
				
				WasmManager.ExecuteMainThreadAction(() => {
					Object.Instantiate(accessed);
				});
			});
		}
	}
}