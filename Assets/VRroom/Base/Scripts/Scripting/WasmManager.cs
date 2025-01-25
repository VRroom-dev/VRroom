using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Wasmtime;

namespace VRroom.Base.Scripting {
	public class WasmManager : MonoBehaviour {
		public static WasmManager Instance { get; private set; }
		public static Config Config { get; private set; }
		public static Engine Engine { get; private set; }
		public static Linker Linker { get; private set; }
		
		private readonly List<WasmVM> _vms = new();
		private readonly Dictionary<string, List<(WasmVM vm, WasmBehaviour behaviour)>> _mainThreadMethods = new();
		private static readonly ConcurrentQueue<Action> MainThreadActions = new();
		private static readonly AutoResetEvent MainThreadSignal = new(false);
		private static readonly AutoResetEvent WorkerSignal = new(false);
		private static volatile bool _workComplete;

		private void Start() {
			Instance = this;
			Config = new Config().WithFuelConsumption(true);
			Engine = new(Config);
			Linker = new Linker(Engine);
			
			// Ideally there would be separate linkers for: Avatars, Props, Worlds, and GameModes each with there own binding set.
			BindingManager.BindMethods(Linker);
		}

		private void ExecuteVMs(string method) {
			_workComplete = false;
			
			Task.Run(() => {
				Parallel.ForEach(_vms, vm => {
					try {
						vm.ResetFuel(1000000);
						vm.ExecuteMethods(method);
					}
					catch (Exception e) {
						Debugging.Console.Exception(e, $"Error executing VM method {method}");
					}
				});
            
				_workComplete = true;
				MainThreadSignal.Set();
			});
			
			while (!_workComplete) {
				MainThreadSignal.WaitOne();
				while (MainThreadActions.TryDequeue(out Action action)) {
					try {
						action();
					} catch (Exception e) {
						Debugging.Console.Exception(e, "Error executing ");
					}
				}
				WorkerSignal.Set();
			}

			if (!_mainThreadMethods.TryGetValue(method, out List<(WasmVM vm, WasmBehaviour behaviour)> methodList)) return;
			foreach ((WasmVM vm, WasmBehaviour behaviour) in methodList) {
				try {
					vm.ResetFuel(100000);
					vm.ExecuteMethod(behaviour, method);
				}
				catch (Exception e) {
					Debugging.Console.Exception(e, $"Error executing main thread method {method}");
				}
			}
		}
		
		public static void ExecuteMainThreadAction(Action action) {
			MainThreadActions.Enqueue(action);
			MainThreadSignal.Set();
			WorkerSignal.WaitOne();
		}

		public void RegisterVM(WasmVM vm) {
			_vms.Add(vm);
			SortMainThreadMethods();
		}

		public void UnregisterVM(WasmVM vm) {
			_vms.Remove(vm);
			SortMainThreadMethods();
		}

		private void SortMainThreadMethods() {
			_mainThreadMethods.Clear();
			
			foreach (var vm in _vms) {
				foreach (var behaviour in vm.GetBehaviours()) {
					foreach (var info in behaviour.executionInfos) {
						if (!info.executeOnMainThread) continue;
						if (!_mainThreadMethods.TryGetValue(info.methodName, out List<(WasmVM vm, WasmBehaviour behaviour)> list)) {
							list = new();
							_mainThreadMethods[info.methodName] = list;
						}
						list.Add((vm, behaviour));
					}
				}
			}
			
			foreach ((string methodName, List<(WasmVM vm, WasmBehaviour behaviour)> list) in _mainThreadMethods) {
				list.Sort((a, b) => {
					var orderA = a.behaviour.executionInfos.Find(x => x.methodName == methodName).executionOrder;
					var orderB = b.behaviour.executionInfos.Find(x => x.methodName == methodName).executionOrder;
					return orderA.CompareTo(orderB);
				});
			}
		}

		private void FixedUpdate() {
			ExecuteVMs("FixedUpdate");
		}

		private void Update() {
			ExecuteVMs("Update");
		}

		private void LateUpdate() {
			ExecuteVMs("LateUpdate");
		}
	}
}