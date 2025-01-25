using UnityEngine;
using VRroom.Base.Scripting;

namespace VRroom.Base {
	public class ContentDescriptor : MonoBehaviour {
		public string guid;
		public WasmModuleAsset module;

		public void Start() {
			if (module != null) {
				gameObject.AddComponent<WasmVM>().moduleAsset = module;
			}
		}
	}
}