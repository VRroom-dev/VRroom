global using static WasmModule.Utils;
using System.Runtime.InteropServices;

namespace WasmModule;
public static unsafe class Utils {
	public static PassthroughArray* ArrayPassthrough;
	
	public static string ReadString() {
		PassthroughArray* arr = ArrayPassthrough;
		string name = new((char*)arr->Address, 0, arr->Size / sizeof(char));
		Marshal.FreeHGlobal((IntPtr)arr->Address);
		return name;
	}

	public static void WriteString(char* str, int length) {
		*ArrayPassthrough = new(str, length * sizeof(char));
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct PassthroughArray(void* address, int size) {
		public void* Address = address;
		public int Size = size;
	}
}