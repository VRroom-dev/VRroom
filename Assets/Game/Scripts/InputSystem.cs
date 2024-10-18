using JetBrains.Annotations;
using UnityEngine;

[PublicAPI]
public class InputSystem : MonoBehaviour {
    public static float MouseSensitivity = 2;
    public static Vector2 Movement { get; private set; }
    public static Vector2 Look { get; private set; }
    public static bool Jump { get; private set; }
    public static bool Crouch { get; private set; }
    public static bool Prone { get; private set; }
    private static bool _cursorLocked;

    private void Start() {
        ToggleCursorLock();
    }

    private void Update() {
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
            Look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * MouseSensitivity;
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