using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRroom.Base.Scripting {
    public class WasmBehaviour : MonoBehaviour {
#if UNITY_EDITOR
        public MonoScript script;
#endif
		public List<WasmVariable<int>> intVariables;
		public List<WasmVariable<bool>> boolVariables;
		public List<WasmVariable<float>> floatVariables;
		public List<WasmVariable<string>> stringVariables;
		public List<WasmVariable<Component>> componentVariables;
		public List<WasmVariable<GameObject>> gameObjectVariables;
		public List<ExecutionInfo> executionInfos;
		public string behaviourName;

		public WasmVM vm;
		private void Awake() => vm.ExecuteMethod(this, "Awake");
		private void Start() => vm.ExecuteMethod(this, "Start");
		private void OnEnable() => vm.ExecuteMethod(this, "OnEnable");
		private void OnDisable() => vm.ExecuteMethod(this, "OnDisable");
		private void OnDestroy() => vm.ExecuteMethod(this, "OnDestroy");
		private void OnPreCull() => vm.ExecuteMethod(this, "OnPreCull");
		private void OnPreRender() => vm.ExecuteMethod(this, "OnPreRender");
		private void OnPostRender() => vm.ExecuteMethod(this, "OnPostRender");
	}
	
	[Serializable]
	public struct WasmVariable<T> {
		public string name;
		public T value;
	}
	
	[Serializable]
	public struct ExecutionInfo {
		public string methodName;
		public int executionOrder;
		public bool executeOnMainThread;
	}
}