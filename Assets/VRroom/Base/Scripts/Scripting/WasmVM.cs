using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Wasmtime;

namespace VRroom.Base.Scripting {
	public class WasmVM : MonoBehaviour {
		private readonly Dictionary<WasmBehaviour, int> _behaviours = new();
		private readonly Dictionary<string, List<WasmBehaviour>> _methods = new();
		private Func<int, int> _allocMethod;
		private Action<int> _createMethod;
		private Action<int> _callMethod;
		private int _arrayPassthrough;
		private Instance _instance;
		private Module _module;
		private Memory _memory;
		private Store _store;
		
		public WasmModuleAsset moduleAsset;

		public void Initialize() {
			_module = Module.FromBytes(WasmManager.Engine, "Scripting", moduleAsset.bytes);
			
			_store = new(WasmManager.Engine);
			_instance = WasmManager.Linker.Instantiate(_store, _module);
			_memory = _instance.GetMemory("memory");
			
			_instance.GetAction("_initialize")?.Invoke();
			_arrayPassthrough = _instance.GetFunction<int>("AllocArrayPassthrough")!();
			
			_allocMethod = _instance.GetFunction<int, int>("Alloc")!;
			_createMethod = _instance.GetAction<int>("CreateInstance")!;
			_callMethod = _instance.GetAction<int>("Call")!;
			
			_store.SetData(new StoreData(gameObject, _arrayPassthrough, _allocMethod, _memory));
			
			foreach (WasmBehaviour behaviour in GetComponentsInChildren<WasmBehaviour>(true)) {
				int id = behaviour.GetInstanceID();
				_behaviours[behaviour] = id;
				CreateInstance(id, behaviour.behaviourName);
				behaviour.vm = this;
			}
			
			foreach (WasmBehaviour behaviour in _behaviours.Keys) {
				foreach (ExecutionInfo info in behaviour.executionInfos) {
					if (info.executeOnMainThread) continue;
					if (!_methods.TryGetValue(info.methodName, out List<WasmBehaviour> list)) {
						list = new();
						_methods[info.methodName] = list;
					}
					list.Add(behaviour);
				}
			}
        
			foreach ((string methodName, List<WasmBehaviour> list) in _methods) {
				list.Sort((a, b) => {
					var orderA = a.executionInfos.Find(x => x.methodName == methodName).executionOrder;
					var orderB = b.executionInfos.Find(x => x.methodName == methodName).executionOrder;
					return orderA.CompareTo(orderB);
				});
			}
			
			WasmManager.Instance.RegisterVM(this);
		}

		public void ExecuteMethods(string name) {
			if (!_methods.TryGetValue(name, out List<WasmBehaviour> behaviours)) return;
			foreach (WasmBehaviour behaviour in behaviours) CallMethod(_behaviours[behaviour], name);
		}

		public void ExecuteMethod(WasmBehaviour behaviour, string name) => CallMethod(_behaviours[behaviour], name);

		public WasmBehaviour[] GetBehaviours() => _behaviours.Keys.ToArray();

		public void ResetFuel(ulong fuel) => _store.Fuel = fuel;

		private void CreateInstance(int id, string name) {
			int address = _allocMethod(name.Length * sizeof(char));
			_memory.WriteString(address, name, Encoding.Unicode);
			_memory.WriteInt32(_arrayPassthrough, address);
			_memory.WriteInt32(_arrayPassthrough + 4, name.Length * sizeof(char));
			_createMethod(id);
		}

		private void CallMethod(int id, string name) {
			int address = _allocMethod(name.Length * sizeof(char));
			_memory.WriteString(address, name, Encoding.Unicode);
			_callMethod(id);
		}

		private void OnDestroy() {
			WasmManager.Instance.UnregisterVM(this);
			_store.Dispose();
			_module.Dispose();
		}
	}

	public readonly struct StoreData {
		private readonly Func<int, int> _allocMethod;
		public readonly GameObject Root;
		public readonly Memory Memory;
		public readonly int ArrayPassthrough;
		
		public StoreData(GameObject root, int arrayPassthrough, Func<int, int> allocMethod, Memory memory) {
			Root = root;
			ArrayPassthrough = arrayPassthrough;
			_allocMethod = allocMethod;
			Memory = memory;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Alloc(int size) => _allocMethod(size);
	}
}