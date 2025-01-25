using System.Runtime.InteropServices;

public static class Debug {
	public static void Log(object obj) {
		string msg = obj.ToString()!;
		unsafe {
			fixed (char* str = msg) {
				WriteString(str, msg.Length);
				debug_log();
			}
		}
	}
	
	public static void LogWarning(object obj) {
		string msg = obj.ToString()!;
		unsafe {
			fixed (char* str = msg) {
				WriteString(str, msg.Length);
				debug_logWarning();
			}
		}
	}
	
	public static void LogError(object obj) {
		string msg = obj.ToString()!;
		unsafe {
			fixed (char* str = msg) {
				WriteString(str, msg.Length);
				debug_logError();
			}
		}
	}
    
	[WasmImportLinkage, DllImport("unity")]
	private static extern void debug_log();
    
	[WasmImportLinkage, DllImport("unity")]
	private static extern void debug_logWarning();
    
	[WasmImportLinkage, DllImport("unity")]
	private static extern void debug_logError();
}