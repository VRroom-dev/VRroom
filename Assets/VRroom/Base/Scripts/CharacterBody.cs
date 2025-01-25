using System;
using UnityEngine;

namespace VRroom.Base {
	[RequireComponent(typeof(Rigidbody))]
	public class CharacterBody : MonoBehaviour {
		public Action<CharacterBody> MovementController { get; private set; }
		public Rigidbody Rigidbody { get; private set; }
		public bool IsVR { get; private set; }

		private Action<CharacterBody> _movementController;
		private Transform _vrCamera;
		private Transform _desktopCamera;
		private Transform _cameraPivot;
		private Vector3 _gravityDir = Vector3.down;
		private float _gravityMag = 9.81f;
		private float _scale = 1;
		private float _pitch;
		
		public Vector3 Gravity {
			get => _gravityDir * _gravityMag;
			set {
				_gravityMag = value.magnitude;
				_gravityDir = value.normalized;
			}
		}
		
		public float Scale {
			get => _scale;
			set {
				_scale = value;
				transform.localScale = Vector3.one * _scale;
			}
		}
		
		public Vector3 CameraPosition => IsVR ? _vrCamera.position : _desktopCamera.position;

		private void Start() {
			Rigidbody = GetComponent<Rigidbody>();
			_vrCamera = transform.Find("VRRig").GetChild(0);
			_cameraPivot = transform.Find("DesktopRig").GetChild(0);
			_desktopCamera = _cameraPivot.GetChild(0);
			GameStateManager.Subscribe("VRMode", (_, vrMode) => IsVR = (bool)vrMode);
		}

		private void Update() {
			Vector2 rotation = 100f * Time.smoothDeltaTime * InputSystem.Look;
		
			if (IsVR) {
				Vector3 camForward = _vrCamera.forward;
				Vector3 forward = Vector3.ProjectOnPlane(camForward, _gravityDir).normalized;
				transform.rotation = Quaternion.LookRotation(forward, -_gravityDir);
				transform.rotation *= Quaternion.AngleAxis(rotation.x, -_gravityDir);
				_vrCamera.forward = camForward;
			} else {
				_pitch -= rotation.y;
				_pitch = Mathf.Clamp(_pitch, -90, 90);
				_cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
				transform.rotation = Quaternion.AngleAxis(rotation.x, -_gravityDir) * transform.rotation;
			}
		}

		private void FixedUpdate() => MovementController?.Invoke(this);
	}
}