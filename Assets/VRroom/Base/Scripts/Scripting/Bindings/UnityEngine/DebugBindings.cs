using Wasmtime;
using Console = VRroom.Base.Debugging.Console;

namespace VRroom.Base.Scripting.UnityEngine {
	public static class DebugBindings {
		public static void BindMethods(Linker linker) {
			linker.DefineFunction("unity", "debug_log", (Caller caller) => {
				StoreData data = BindingHelpers.GetData(caller);
				string msg = BindingHelpers.ReadString(data);
				Console.Log(msg);
			});
			
			linker.DefineFunction("unity", "debug_logWarning", (Caller caller) => {
				StoreData data = BindingHelpers.GetData(caller);
				string msg = BindingHelpers.ReadString(data);
				Console.Warn(msg);
			});
			
			linker.DefineFunction("unity", "debug_logError", (Caller caller) => {
				StoreData data = BindingHelpers.GetData(caller);
				string msg = BindingHelpers.ReadString(data);
				Console.Error(msg);
			});
			
			linker.DefineFunction("unity", "debug_logException", (Caller caller) => {
				StoreData data = BindingHelpers.GetData(caller);
				string msg = BindingHelpers.ReadString(data);
				Console.Exception(msg);
			});
		}
	}
}