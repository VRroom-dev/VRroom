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
    public bool IsGrounded = true;
    
    private InputSystem _inputSystem;
    private float _groundedTime;
    private const float GroundedDist = 0.05f;
    private const float SkinWidth = 0.015f;
    private const int MaxBounces = 5;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start() {
        _inputSystem = InputSystem.Instance;
    }

    private void Update() {
        Vector3 movement = new(_inputSystem.Movement.x, 0, _inputSystem.Movement.y);
        if (_inputSystem.Jump && _groundedTime > 0.1f) {
            IsGrounded = false;
            velocity += -gravity.normalized * Mathf.Sqrt(5 * gravity.magnitude * jumpHeight);
        }

        movement *= acceleration;
        float value = 1 - Mathf.Clamp01(new Vector3(velocity.x, 0, velocity.z).magnitude / speed);
        movement = Vector3.Lerp(Vector3.ProjectOnPlane(movement, new Vector3(velocity.x, 0, velocity.z)), movement, value); //dont accelerate faster while not grounded

        if (!IsGrounded) movement *= 0.1f; //reduce controllability in air

        Vector3 moveAmount = velocity * Time.deltaTime;
        moveAmount += movement * Time.deltaTime;

        if (IsGrounded) moveAmount *= 1 - (friction * Time.deltaTime); //friction
        moveAmount += -moveAmount.normalized * (drag * moveAmount.magnitude * moveAmount.magnitude * Time.deltaTime); //drag

        if (moveAmount.magnitude > 0.0000000001) { //move player
            moveAmount = CollideAndSlide(moveAmount, transform.position, false);
            moveAmount += CollideAndSlide(gravity * (0.01f * Time.deltaTime), transform.position + moveAmount, true);
            transform.position += moveAmount;
        }

        velocity = moveAmount / Time.deltaTime;

        Vector3 point = -gravity.normalized * radius + transform.position;
        IsGrounded = Physics.SphereCast(point, radius - SkinWidth, gravity, out _, GroundedDist); //grounded check
        
        if (IsGrounded) _groundedTime += Time.deltaTime;
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
                if (IsGrounded && !gravityPass) //treat as vertical wall if grounded
                    leftover = Vector3.ProjectOnPlane(new Vector3(leftover.x, 0, leftover.z), new Vector3(hit.normal.x, 0, hit.normal.z));
                else leftover = Vector3.ProjectOnPlane(leftover, hit.normal);
            }

            return snapToSurface + CollideAndSlide(leftover, pos + snapToSurface, gravityPass, depth + 1);
        }

        return vel;
    }
}
