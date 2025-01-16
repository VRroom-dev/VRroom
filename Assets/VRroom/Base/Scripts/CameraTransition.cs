using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRroom.Base {
	[PublicAPI]
	public class CameraTransition : MonoBehaviour {
		private static readonly int TransitionAmount = Shader.PropertyToID("_TransitionAmount");
		private static CommandBuffer _transitionBuffer;
		private static Material _transitionMaterial;
		private static Mesh _quad;
		private static Camera _camera;
		private static bool _initialized;
		private bool _bufferAttached;
		
		public static bool Transition(bool active) => _transition = active;
		private static float _transitionTime = 1;
		private static bool _transition = true;
		public static float TransitionSpeed = 1;
		public static bool Transitioning {
			get {
				bool transitioning = false;
				if (_transition && _transitionTime < 1) transitioning = true;
				else if (!_transition && _transitionTime > 0) transitioning = true;
				return transitioning;
			}
		}

		public void Start() {
			_camera = GetComponent<Camera>();
			
			if (_initialized) return;
			_transitionMaterial = new(Shader.Find("Hidden/FadeTransition"));
			_quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
			_transitionBuffer = new();
			_transitionBuffer.DrawMesh(_quad, Matrix4x4.identity, _transitionMaterial);
			_initialized = true;
		}

		public void Update() {
			if (_transition && !_bufferAttached) {
				_camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _transitionBuffer);
				_bufferAttached = true;
			} else if (!_transition && _bufferAttached) {
				_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _transitionBuffer);
				_bufferAttached = false;
			}
			
			if (!Transitioning) return;
			_transitionTime = Mathf.Clamp01(_transitionTime + (_transition ? Time.deltaTime : -Time.deltaTime) * TransitionSpeed);
			_transitionMaterial.SetFloat(TransitionAmount, _transitionTime);
		}

		public void OnDestroy() {
			if (!_initialized) return;
			Destroy(_transitionMaterial);
			_transitionBuffer.Release();
			_initialized = false;
		}
	}
}