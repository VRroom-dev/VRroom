using UnityEngine;

public class CharacterController : MonoBehaviour {
    public static CharacterController Instance;
    public float height = 1.6f;
    public float radius = 0.2f;
    public float acceleration = 0.5f;
    public float speed = 10;
    public float jumpHeight = 1;
    public float drag = 5;
    public float friction = 10;
    public float maxSlope = 55;
    public Vector3 gravity;
    public Vector3 velocity;
    public bool isGrounded = true;
    
    private InputSystem _inputSystem;
    private Transform _cameraPivot;
    private Transform _cameraAnchor; // avatar view point anchor (head bone)
    private bool _isVR;
    private float _groundedTime;
    private const float GroundedDist = 0.05f;
    private const float SkinWidth = 0.015f;
    private const int MaxBounces = 5;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _inputSystem = InputSystem.Instance;
        _cameraPivot = transform.Find("DesktopRig").GetChild(0);
        _isVR = (bool)GameStateManager.Instance["VRMode"];
        GameStateManager.Subscribe("VRMode", (_, vrMode) => { _isVR = (bool)vrMode; });
    }

    private void Update()
    {
        transform.Rotate(-gravity, _inputSystem.Look.x * 100 * Time.deltaTime, Space.World);
        if (!_isVR) _cameraPivot.transform.Rotate(Vector3.right, -_inputSystem.Look.y * 100 * Time.deltaTime, Space.Self);
        else _cameraPivot.rotation = _cameraAnchor.rotation;
        
        if (_cameraAnchor) {
            _cameraPivot.position = _cameraAnchor.position;
            if (!_isVR) _cameraAnchor.rotation = _cameraPivot.rotation;
        }
    }
    
    private void FixedUpdate() {
        Vector3 movement = transform.rotation * new Vector3(_inputSystem.Movement.x, 0, _inputSystem.Movement.y);        
        if (_inputSystem.Jump && _groundedTime > 0.1f) {
            isGrounded = false;
            _groundedTime = 0;
            velocity += -gravity.normalized * Mathf.Sqrt(5 * gravity.magnitude * jumpHeight);
        }

        movement *= acceleration;
        float value = 1 - Mathf.Clamp01(new Vector3(velocity.x, 0, velocity.z).magnitude / speed);
        movement = Vector3.Lerp(Vector3.ProjectOnPlane(movement, new Vector3(velocity.x, 0, velocity.z)), movement, value); //dont accelerate faster while not grounded

        if (!isGrounded) movement *= 0.1f; //reduce controllability in air

        Vector3 moveAmount = velocity * Time.deltaTime;
        moveAmount += movement * Time.deltaTime;

        if (isGrounded) moveAmount *= 1 - (friction * Time.deltaTime); //friction
        moveAmount += -moveAmount.normalized * (drag * moveAmount.magnitude * moveAmount.magnitude * Time.deltaTime); //drag

        if (moveAmount.magnitude > 0.0000000001) { //move player
            moveAmount = CollideAndSlide(moveAmount, transform.position, false);
            moveAmount += CollideAndSlide(gravity * (0.01f * Time.deltaTime), transform.position + moveAmount, true);
            transform.position += moveAmount;
        }

        velocity = moveAmount / Time.deltaTime;

        Vector3 point = -gravity.normalized * radius + transform.position;
        isGrounded = Physics.SphereCast(point, radius - SkinWidth, gravity, out _, GroundedDist); //grounded check
        
        if (isGrounded) _groundedTime += Time.deltaTime;
        else _groundedTime = 0;
    }

    private Vector3 CollideAndSlide(Vector3 vel, Vector3 pos, bool gravityPass, int depth = 0) {
        if (depth > MaxBounces) return Vector3.zero;
        float dist = vel.magnitude + SkinWidth;
        Vector3 point1 = pos + -gravity.normalized * (height - radius);
        Vector3 point2 = pos + -gravity.normalized * radius;

        if (Physics.CapsuleCast(point1, point2, radius, vel, out RaycastHit hit, dist)) {
            Vector3 snapToSurface = vel.normalized * (hit.distance - SkinWidth);
            if (snapToSurface.magnitude <= SkinWidth) snapToSurface = Vector3.zero;
            Vector3 leftover = vel - snapToSurface;
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            if (angle <= maxSlope) { //normal ground
                if (gravityPass) return snapToSurface;
                leftover = Vector3.ProjectOnPlane(leftover, hit.normal);
            }
            else { //steep slope
                if (isGrounded && !gravityPass) //treat as vertical wall if grounded
                    leftover = Vector3.ProjectOnPlane(new Vector3(leftover.x, 0, leftover.z), new Vector3(hit.normal.x, 0, hit.normal.z));
                else leftover = Vector3.ProjectOnPlane(leftover, hit.normal);
            }

            return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, gravityPass, depth + 1);
        }

        return vel;
    }
}
