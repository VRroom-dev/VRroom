using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Wasmtime;
using Object = UnityEngine.Object;

namespace VRroom.Base.Scripting {
	public static class BindingHelpers {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetAccessed<T>(Caller caller, int objectId, bool reading, out T accessed) where T : Object {
			StoreData data = GetData(caller);
			GameObject root = data.Root;
			accessed = BindingManager.GetObjectById<T>(objectId);
			BindingManager.ThrowIfCantAccessObject(root, accessed, reading);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetAccessed<T>(Caller caller, int objectId, bool reading, out T accessed, out StoreData data) where T : Object {
			data = GetData(caller);
			GameObject root = data.Root;
			accessed = BindingManager.GetObjectById<T>(objectId);
			BindingManager.ThrowIfCantAccessObject(root, accessed, reading);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetAccessed<T>(Caller caller, int objectId, bool reading, out T accessed, out GameObject root) where T : Object {
			StoreData data = GetData(caller);
			root = data.Root;
			accessed = BindingManager.GetObjectById<T>(objectId);
			BindingManager.ThrowIfCantAccessObject(root, accessed, reading);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ReadString(StoreData data) {
			Memory memory = data.Memory;
			int passthrough = data.ArrayPassthrough;
			int address = memory.ReadInt32(passthrough);
			int size = memory.ReadInt32(passthrough + 4);
			return memory.ReadString(address, size, Encoding.Unicode);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteString(StoreData data, string value) {
			int size = value.Length * sizeof(char);
			int address = data.Alloc(size);
			int passthrough = data.ArrayPassthrough;
			Memory memory = data.Memory;
			memory.WriteString(address, value, Encoding.Unicode);
			memory.WriteInt32(passthrough, address);
			memory.WriteInt32(passthrough + 4, size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StoreData GetData(Caller caller) => (StoreData)caller.Store.GetData()!;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Object ReturnTrickled(GameObject root, Object parent, Object child) {
			BindingManager.TricklePermissions(root, parent, child);
			BindingManager.TrackObject(child);
			return child;
		}
	}
}