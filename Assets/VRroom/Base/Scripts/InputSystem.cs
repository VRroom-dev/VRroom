using JetBrains.Annotations;
using UnityEngine;

namespace VRroom.Base {
    [PublicAPI]
    public class InputSystem : MonoBehaviour {
        public static float MouseSensitivity = 2;
        public static Vector2 Movement { get; private set; }
        public static Vector2 Look { get; private set; }
        public static bool Jump { get; private set; }
        public static bool Crouch { get; private set; }
        public static bool Prone { get; private set; }
        public static bool Sprint { get; private set; }
        private static bool _cursorLocked;

        private void Start() {
            SetCursorLock(true);
        }

        private void Update() {
            Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKeyDown(KeyCode.Escape)) SetCursorLock(!_cursorLocked);
            Jump = Input.GetKey(KeyCode.Space);
            Sprint = Input.GetKey(KeyCode.LeftShift);
        
            if (Input.GetKeyDown(KeyCode.C)) {
                Crouch = !Crouch;
                Prone = false;
            }
            if (Input.GetKeyDown(KeyCode.X)) {
                Prone = !Prone;
                Crouch = false;
            }

            if (_cursorLocked) {
                Look = new Vector2(Input.GetAxis("Mouse X") * 2, Input.GetAxis("Mouse Y")) * MouseSensitivity;
            }
        }

        public static void SetCursorLock(bool state) {
            _cursorLocked = state;

            if (_cursorLocked) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Look = Vector2.zero;
            }
        }
    }
}