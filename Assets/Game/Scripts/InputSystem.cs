using UnityEngine;

public class InputSystem : MonoBehaviour {
    public static InputSystem Instance;
    public float mouseSensitivity = 2;
    public Vector2 Movement { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Jump { get; private set; }
    public bool Crouch;
    public bool Prone ;
    private bool _cursorLocked;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void Start() {
        ToggleCursorLock();
    }

    void Update() {
        Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursorLock();
        Jump = Input.GetKey(KeyCode.Space);
        
        if (Input.GetKeyDown(KeyCode.C)) {
            Crouch = !Crouch;
            Prone = false;
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            Prone = !Prone;
            Crouch = false;
        }

        if (_cursorLocked) {
            Look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;
        }
    }

    public void ToggleCursorLock() {
        _cursorLocked = !_cursorLocked;

        if (_cursorLocked) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}