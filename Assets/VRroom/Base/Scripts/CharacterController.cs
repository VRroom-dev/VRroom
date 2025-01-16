using UnityEngine;

namespace VRroom.Base {
	public class CharacterController : MonoBehaviour {
		public float acceleration = 50f;
		public float walkSpeed = 4f;
		public float runSpeed = 8f;
		public float maxSlope = 55f;
		public float jumpHeight = 1f;
		public float uprightSpeed = 5f;
		public float lookSpeed = 2;
		public LayerMask ground;
	
		public Vector3 Gravity {
			get => _gravityDir * _gravityMag;
			set {
				_gravityMag = value.magnitude;
				_gravityDir = value.normalized;
			}
		}

		private Transform _camera;
		private Transform _cameraPivot;
		private CapsuleCollider _collider;
		private Rigidbody _rigidbody;
		private Collider[] _overlaps;
		private Vector3 _movement;
		private Vector3 _groundNormal;
		private float _pitch;
		private bool _wasGrounded;
		private bool _grounded;
		private bool _isVR;
	
		private Vector3 _gravityDir = Vector3.down;
		private float _gravityMag = 9.81f;

		private void Awake() {
			_collider = GetComponent<CapsuleCollider>();
			_rigidbody = GetComponent<Rigidbody>();
			_overlaps = new Collider[8];
			_camera = Camera.main!.transform;
			_cameraPivot = transform.Find("DesktopRig").GetChild(0);
			GameStateManager.Subscribe("VRMode", (_, vrMode) => {
				_isVR = (bool)vrMode;
				_camera = Camera.main.transform;
			});
		}

		private void Update() {
			Vector2 rotation = 100f * lookSpeed * Time.smoothDeltaTime * InputSystem.Look;
		
			if (_isVR) {
				Vector3 camForward = _camera.forward;
				Vector3 forward = Vector3.ProjectOnPlane(camForward, _gravityDir).normalized;
				transform.forward = forward;
				_camera.forward = camForward;
				transform.position = _camera.position;
				_camera.position = transform.position;
			} else {
				_pitch -= rotation.y;
				_pitch = Mathf.Clamp(_pitch, -90, 90);
				_cameraPivot.localRotation = Quaternion.Euler(_pitch, _cameraPivot.localEulerAngles.y, _cameraPivot.localEulerAngles.z);
			}
		
			// look rotation
			transform.Rotate(-_gravityDir, rotation.x, Space.World);
			
			// gravity alignment
			Quaternion currentRotation = transform.rotation;
			Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -_gravityDir) * currentRotation;
			_rigidbody.MoveRotation(Quaternion.Slerp(currentRotation, targetRotation, uprightSpeed * Time.deltaTime));
		}

		private void FixedUpdate() {
			Vector3 velocity = _rigidbody.linearVelocity;
		
			// grounding
			if (_grounded) {
				velocity = Vector3.ProjectOnPlane(velocity, _groundNormal);
			} else if (_wasGrounded) {
				Vector3 p1 = transform.position + _collider.center + Vector3.up * (_collider.height / 2 - _collider.radius);
				Vector3 p2 = transform.position + _collider.center - Vector3.up * (_collider.height / 2 - _collider.radius);
			
				int count = Physics.OverlapCapsuleNonAlloc(p1, p2, _collider.radius, _overlaps, ground);
				float closestDistance = float.MaxValue;
				Collider closestCollider = null;
				for (int i = 0; i < count; i++) {
					if (!Physics.ComputePenetration(_collider, transform.position, transform.rotation, _overlaps[i], _overlaps[i].transform.position, _overlaps[i].transform.rotation, out _, out float distance)) continue;
					if (distance > closestDistance) continue;
					closestDistance = distance;
					closestCollider = _overlaps[i];
				}
        
				if (closestCollider) {
					Physics.ComputePenetration(_collider, transform.position, transform.rotation, closestCollider, closestCollider.transform.position, closestCollider.transform.rotation, out Vector3 resolveDir, out float resolveDistance);
					transform.position += resolveDir * resolveDistance;
					if (Vector3.Angle(resolveDir, -_gravityDir) < maxSlope) {
						velocity = Vector3.ProjectOnPlane(velocity, resolveDir);
						_grounded = true;
					}
				} else if (_rigidbody.SweepTest(_gravityDir, out RaycastHit hit, 1f)) {
					transform.position += _gravityDir * hit.distance;
					velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
					_grounded = true;
				}
			}
		
			Vector3 verticalVelocity = Vector3.Project(velocity, _gravityDir);
			Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, _gravityDir);
		
			// gravity
			if (!_grounded) verticalVelocity += Gravity * Time.fixedDeltaTime;
		
			// movement
			float speed = _isVR ? runSpeed : InputSystem.Sprint ? runSpeed : walkSpeed;
			Vector2 input = Vector2.ClampMagnitude(InputSystem.Movement, 1f);
			Vector3 movement = (transform.right * input.x + transform.forward * input.y).normalized * speed;
			movement = Vector3.ProjectOnPlane(movement, _gravityDir);
			horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, movement, acceleration * Time.fixedDeltaTime);
		
			// jump
			if (_grounded && InputSystem.Jump) {
				verticalVelocity = -_gravityDir * Mathf.Sqrt(2f * jumpHeight * _gravityMag);
				_wasGrounded = false;
				_grounded = false;
			}
		
			_rigidbody.linearVelocity = horizontalVelocity + verticalVelocity;
			_wasGrounded = _grounded;
			_grounded = false;
		}
	
		private void OnCollisionEnter(Collision other) => HandleCollision(other);
		private void OnCollisionStay(Collision other) => HandleCollision(other);
    
		private void HandleCollision(Collision other) {
			Vector3 normals = Vector3.zero;
			int normalCount = 0;
			for (int i = 0; i < other.contactCount; i++) {
				if (Vector3.Angle(other.contacts[i].normal, -_gravityDir) > maxSlope) continue;
				normals += other.contacts[i].normal;
				normalCount++;
			}

			if (normalCount > 0) {
				_groundNormal = normals / normalCount;
				_grounded = true;
			} else {
				_grounded = false;
			}
		}
	}
}