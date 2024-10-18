using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CustomCharacterController : MonoBehaviour {
	public float acceleration = 10f;
	public float maxSpeed = 10f;
	public float maxSlope = 55f;
	public float jumpHeight = 1f;
	public float uprightSpeed = 5f;
	public float lookSpeed = 2;
	public float groundedDistance = 0.01f;
	public LayerMask groundMask;
	
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
	private Vector3 _movement;
	private Vector3 _velocity;
	private float _pitch;
	private bool _wasGrounded;
	private bool _grounded;
	private bool _isVR;
	
	private Vector3 _gravityDir = Vector3.down;
	private float _gravityMag = 9.81f;

	private void Awake() {
		_collider = GetComponent<CapsuleCollider>();
		_rigidbody = GetComponent<Rigidbody>();
		_camera = Camera.main!.transform;
		_cameraPivot = transform.Find("DesktopRig").GetChild(0);
		GameStateManager.Subscribe("VRMode", (_, vrMode) => {
			_isVR = (bool)vrMode;
			_camera = Camera.main.transform;
		});
	}

	private void Update() {
		Vector2 rotation = 100f * lookSpeed * Time.deltaTime * InputSystem.Look;
		
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
		
		transform.Rotate(-_gravityDir, rotation.x, Space.World);
	}

	private void FixedUpdate() {
		_velocity = _rigidbody.linearVelocity;
		
		// gravity
		if (!_grounded) _velocity += Gravity * Time.fixedDeltaTime;
		
		Vector3 verticalVelocity = Vector3.Project(_velocity, _gravityDir);
		Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_velocity, _gravityDir);
		
		// movement
		Vector2 input = Vector2.ClampMagnitude(InputSystem.Movement, 1f) * maxSpeed;
		Vector3 movement = (transform.right * input.x + transform.forward * input.y).normalized * maxSpeed;
		movement = Vector3.ProjectOnPlane(movement, _gravityDir);
		horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, movement, acceleration * Time.fixedDeltaTime);
		
		_velocity = horizontalVelocity + verticalVelocity;

		// grounding
		_wasGrounded = _grounded;
		if (Physics.SphereCast(transform.position + transform.up * (_collider.height - _collider.radius), _collider.radius, _gravityDir, out RaycastHit hit, 1f, groundMask)) {
			_grounded = hit.distance - (_collider.height - _collider.radius * 2f) <= groundedDistance;
			
			if (_grounded) {
				transform.position += _gravityDir * (hit.distance - (_collider.height - _collider.radius * 2f));

				if (Vector3.Angle(hit.normal, -_gravityDir) <= maxSlope) {
					_velocity = Vector3.ProjectOnPlane(_velocity, hit.normal);
				}
			}
			else if (_wasGrounded) {
				if (Vector3.Angle(hit.normal, -_gravityDir) <= maxSlope) {
					transform.position += _gravityDir * (hit.distance - (_collider.height - _collider.radius * 2f));
					_velocity = Vector3.ProjectOnPlane(_velocity, hit.normal);
					_grounded = true;
				}
			}
		} else _grounded = false;
		
		// jump
		if (_grounded && InputSystem.Jump) {
			_velocity = Vector3.ProjectOnPlane(_velocity, _gravityDir);
			_velocity -= _gravityDir * Mathf.Sqrt(2f * jumpHeight * _gravityMag);
			_wasGrounded = false;
			_grounded = false;
		}
		
		_rigidbody.linearVelocity = _velocity;

		// gravity alignment
		Quaternion currentRotation = transform.rotation;
		Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -_gravityDir) * currentRotation;
		_rigidbody.MoveRotation(Quaternion.Slerp(currentRotation, targetRotation, uprightSpeed * Time.fixedDeltaTime));
	}
}