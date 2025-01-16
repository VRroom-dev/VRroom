using UnityEngine;

namespace VRroom.Base {
	public class CoroutineRunner : MonoBehaviour {
		public static CoroutineRunner Instance;
		public CoroutineRunner() => Instance = this;
	}
}