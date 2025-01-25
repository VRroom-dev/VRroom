using UnityEngine;

namespace VRroom.Base {
	public class CharacterController : MonoBehaviour {
		public float acceleration = 50f;
		public float walkSpeed = 4f;
		public float runSpeed = 8f;
		public float maxSlope = 55f;
		public float maxStepHeight = 0.5f;
		public float jumpHeight = 1f;
		public float uprightSpeed = 5f;
		public float skinWidth = 0.01f;
		public float groundedDistance = 0.01f;
		public float pushForce = 5f;
		public LayerMask ground;
	
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

		private Transform _camera;
		private Transform _cameraPivot;
		private CapsuleCollider _collider;
		private Rigidbody _rigidbody;
		private Collider[] _colliders;
		private Quaternion _rotation;
		private Vector3 _position;
		private Vector3 _velocity;
		private float _pitch;
		private bool _wasGrounded;
		private bool _grounded;
		private bool _isVR;
	
		[SerializeField]
		private Vector3 _gravityDir = Vector3.down;
		private float _gravityMag = 9.81f;
		private float _scale = 1;
		private float _radius;
		private float _height;

		private void Start() {
			_collider = GetComponent<CapsuleCollider>();
			_rigidbody = GetComponent<Rigidbody>();
			_camera = Camera.main!.transform;
			_cameraPivot = transform.Find("DesktopRig").GetChild(0);
			_rotation = Quaternion.identity;
			_colliders = new Collider[8];
			
			GameStateManager.Subscribe("VRMode", (_, vrMode) => {
				_isVR = (bool)vrMode;
				_camera = Camera.main.transform;
			});

			_rigidbody.isKinematic = true;
			Time.fixedDeltaTime = 0.016f;
		}

		private void Update() {
			Vector2 rotation = 100f * Time.smoothDeltaTime * InputSystem.Look;
		
			if (_isVR) {
				Vector3 camForward = _camera.forward;
				Vector3 forward = Vector3.ProjectOnPlane(camForward, _gravityDir).normalized;
				_rotation = Quaternion.LookRotation(forward, -_gravityDir);
				_rotation *= Quaternion.AngleAxis(rotation.x, -_gravityDir);
				
				transform.rotation = _rotation;
				_camera.forward = camForward;
				transform.position = _camera.position;
				_camera.position = transform.position;
			} else {
				 _pitch -= rotation.y;
	            _pitch = Mathf.Clamp(_pitch, -90, 90);
	            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
				_rotation = Quaternion.AngleAxis(rotation.x, -_gravityDir) * _rotation;
			}
			
			// gravity alignment
            Quaternion targetUp = Quaternion.FromToRotation(_rotation * Vector3.up, -_gravityDir) * _rotation;
			_rotation = Quaternion.Slerp(_rotation, targetUp, uprightSpeed * Time.deltaTime);
			_rigidbody.rotation = _rotation;
		}

		private void FixedUpdate() {
			_gravityDir = _gravityDir.normalized;
			_height = _collider.height;
			_radius = _collider.radius;

			Depenetrate();
			TryGround();
			
			Vector3 verticalVelocity = Vector3.Project(_velocity, _gravityDir);
			Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_velocity, _gravityDir);
			
			// movement
			float speed = _isVR ? runSpeed : InputSystem.Sprint ? runSpeed : walkSpeed;
			Vector3 input = Vector3.ClampMagnitude(InputSystem.Movement, 1f);
			if (_grounded || input.magnitude > 0.01f) {
				Vector3 movement = (_rotation * Vector3.right * input.x + _rotation * Vector3.forward * input.z).normalized * speed;

				if (!_grounded) {
					float currentSpeed = horizontalVelocity.magnitude;
					horizontalVelocity += movement * (acceleration * Time.fixedDeltaTime * 0.1f);

					if (currentSpeed > speed && horizontalVelocity.magnitude > currentSpeed) {
						horizontalVelocity = horizontalVelocity.normalized * currentSpeed;
					} else if (horizontalVelocity.magnitude > speed) {
						horizontalVelocity = horizontalVelocity.normalized * speed;
					}
				} 
				else horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, movement, acceleration * Time.fixedDeltaTime);
			}
			
			// gravity
			if (!_grounded) verticalVelocity += Gravity * Time.fixedDeltaTime;
			
			// jump
			if (_grounded && InputSystem.Jump) {
				verticalVelocity = -_gravityDir * Mathf.Sqrt(2f * jumpHeight * _gravityMag);
				_wasGrounded = false;
				_grounded = false;
			}

			_velocity = horizontalVelocity + verticalVelocity;
			Vector3 displacement = CollideAndSlide(_velocity * Time.fixedDeltaTime, _position);
			_velocity = displacement / Time.fixedDeltaTime;
			_position += displacement;
			
			_rigidbody.MovePosition(_position);
		}

		private void TryGround() {
			_wasGrounded = _grounded;
			_grounded = false;
			if (CastSelf(_position, _gravityDir, maxStepHeight * 2, out RaycastHit groundHit)) {
				_grounded = groundHit.distance < groundedDistance && ValidGround(groundHit.normal);
				if (!_grounded && _wasGrounded && Physics.Raycast(_position - _gravityDir * skinWidth, _gravityDir, out RaycastHit groundHit2, maxStepHeight * 2 + skinWidth, ground) && ValidGround(groundHit2.normal)) {
					_position += _gravityDir * Vector3.Dot(groundHit.point - _position, _gravityDir);
					if (ValidGround(groundHit.normal)) {
						_velocity = Vector3.ProjectOnPlane(_velocity, _gravityDir);
						_velocity += _gravityDir / Vector3.Dot(groundHit.normal, _gravityDir) + _gravityDir;
					}
					_grounded = true;
				}
			}
		}

		private bool ValidGround(Vector3 normal) => Vector3.Angle(normal, -_gravityDir) < maxSlope;
		private float GetVerticalOffset(Vector3 point) => Vector3.Project(point - _position, _gravityDir).magnitude;

		private bool CastSelf(Vector3 pos, Vector3 dir, float dist, out RaycastHit hit) {
			float radius = _radius * _scale - skinWidth;
			Vector3 point1 = pos + -_gravityDir * _radius;
			Vector3 point2 = pos + -_gravityDir * (_height - _radius);
			bool didHit = Physics.CapsuleCast(point1, point2, radius, dir, out hit, dist + skinWidth, ground);
			if (didHit) hit.distance = DistanceToPoint(hit.point, point1, point2) - (radius + skinWidth);
			return didHit;
		}

		private void Depenetrate() {
			Vector3 point1 = _position + -_gravityDir * _radius;
			Vector3 point2 = _position + -_gravityDir * (_height - _radius);
			int count = Physics.OverlapCapsuleNonAlloc(point1, point2, _radius * _scale * 2f, _colliders, ground);
			for (int i = 0; i < count; i++) {
				Collider col = _colliders[i];
				if (col == _collider) continue;
				if (Physics.ComputePenetration(_collider, _position, _rotation, col, col.transform.position, col.transform.rotation, out Vector3 direction, out float distance)) {
					_position += direction * distance;
				}
			}
		}
		
		private Vector3 CollideAndSlide(Vector3 vec, Vector3 pos, int depth = 0) {
			if (depth > 5) return Vector3.zero;
		
			Vector3 dir = vec.normalized;
			if (!CastSelf(pos, dir, vec.magnitude, out RaycastHit hit)) return vec;
			Vector3 snapToSurface = dir * hit.distance;
			Vector3 leftover = vec - snapToSurface;
			
			if (hit.rigidbody && !hit.rigidbody.isKinematic) {
		        hit.rigidbody.AddForce(snapToSurface * (pushForce * 1000f), ForceMode.Force);
		    }
			
			leftover = Vector3.ProjectOnPlane(leftover, hit.normal);
			return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, depth + 1);
		}

		private static float DistanceToPoint(Vector3 p, Vector3 a, Vector3 b) {
			Vector3 ap = p - a;
			Vector3 ab = b - a;
			float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude);
			return Vector3.Distance(a + t * ab, p);
		}
	}
}