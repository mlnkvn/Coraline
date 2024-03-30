using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Coraline {
    [CreateAssetMenu(fileName = "InputReader", menuName = "Coraline/InputReader")]
    public class InputReader : ScriptableObject, IPlayerActions {
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        
        public event UnityAction<bool> Pick = delegate { };
        public event UnityAction<bool> LookThroughStone = delegate { };

        private PlayerInputActions _inputActions;
        
        public Vector3 Direction => _inputActions.Player.Move.ReadValue<Vector2>();

        private void OnEnable()
        {
            if (_inputActions != null) return;
            _inputActions = new PlayerInputActions();
            _inputActions.Player.SetCallbacks(this);
        }
        
        public void EnablePlayerActions() {
            _inputActions.Enable();
        }

        public void OnMove(InputAction.CallbackContext context) {
            Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context) {
            Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }

        private static bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";

        public void OnFire(InputAction.CallbackContext context) {
        }

        public void OnRun(InputAction.CallbackContext context) {
        }

        public void OnJump(InputAction.CallbackContext context) {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnPick(InputAction.CallbackContext context)
        {
            switch (context.phase) {
                case InputActionPhase.Started:
                    Pick.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Pick.Invoke(false);
                    break;
            }
        }
        
        
        public void OnLookThroughStone(InputAction.CallbackContext context)
        {
            switch (context.phase) {
                case InputActionPhase.Started:
                    LookThroughStone.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    LookThroughStone.Invoke(false);
                    break;
            }
        }
    }
}