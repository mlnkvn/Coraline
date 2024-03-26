using System.Collections.Generic;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace Coraline {
    public class PlayerController : ValidatedMonoBehaviour {
        [Header("References")]
        [SerializeField, Self] private Rigidbody rb;
        [SerializeField, Self] private GroundChecker groundChecker;
        [SerializeField, Self] private Animator animator;
        [SerializeField, Anywhere] private CinemachineFreeLook freeLookVCam;
        [SerializeField, Anywhere] private InputReader input;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.2f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float jumpDuration = 0.4f;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float gravityMultiplier = 7f;
        

        private const float ZeroF = 0f;
        
        private Transform _mainCam;
        
        private float _currentSpeed;
        private float _velocity;
        private float _jumpVelocity;
        
        private Vector3 _movement;

        private List<Timer> _timers;
        private CountdownTimer _jumpTimer;
        private CountdownTimer _jumpCooldownTimer;
        
        private StateMachine _stateMachine;
        
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake() {
            if (Camera.main != null) _mainCam = Camera.main.transform;
            var varTransform = transform;
            freeLookVCam.Follow = varTransform;
            freeLookVCam.LookAt = varTransform;
            freeLookVCam.OnTargetObjectWarped(varTransform, varTransform.position - freeLookVCam.transform.position - Vector3.forward);

            rb.freezeRotation = true;
            
            SetupTimers();
            SetupStateMachine();
        }

        private void SetupStateMachine() {
            _stateMachine = new StateMachine();
            
            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            
            At(locomotionState, jumpState, new FuncPredicate(() => _jumpTimer.IsRunning));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));
            
            _stateMachine.SetState(locomotionState);
        }

        private bool ReturnToLocomotionState()
        {
            return groundChecker.IsGrounded && !_jumpTimer.IsRunning;
        }

        void SetupTimers() {
            // Setup timers
            _jumpTimer = new CountdownTimer(jumpDuration);
            _jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            _jumpTimer.OnTimerStart += () => _jumpVelocity = jumpForce;
            _jumpTimer.OnTimerStop += () => _jumpCooldownTimer.Start();

            _timers = new List<Timer>(2) {_jumpTimer, _jumpCooldownTimer};
        }

        private void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
        private void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

        private void Start() => input.EnablePlayerActions();

        private void OnEnable() {
            input.Jump += OnJump;
        }
        
        private void OnDisable() {
            input.Jump -= OnJump;
        }
        private void OnJump(bool performed) {
            if (performed && !_jumpTimer.IsRunning && !_jumpCooldownTimer.IsRunning && groundChecker.IsGrounded) {
                _jumpTimer.Start();
            } else if (!performed && _jumpTimer.IsRunning) {
                _jumpTimer.Stop();
            }
        }

        void Update() {
            _movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            _stateMachine.Update();

            HandleTimers();
            UpdateAnimator();
        }

        void FixedUpdate() {
            _stateMachine.FixedUpdate();
        }

        void UpdateAnimator() {
            animator.SetFloat(Speed, _currentSpeed);
        }

        void HandleTimers() {
            foreach (var timer in _timers) {
                timer.Tick(Time.deltaTime);
            }
        }

        public void HandleJump() {
            if (!groundChecker.IsGrounded) return;
            
            if (!_jumpTimer.IsRunning && groundChecker.IsGrounded) {
                _jumpVelocity = ZeroF;
                return;
            }
            
            if (!_jumpTimer.IsRunning) {
                _jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }


            var velocity = rb.velocity;
            rb.velocity = new Vector3(velocity.x, _jumpVelocity, velocity.z);
        }

        public void HandleMovement()
        {
            var cameraRotation = _mainCam.rotation;
            cameraRotation.y = 0;
            var adjustedDirection = Quaternion.AngleAxis(_mainCam.eulerAngles.y, Vector3.up) * _movement;
            var finalDirection = cameraRotation * adjustedDirection;
            finalDirection.y = adjustedDirection.y;
            // adjust direction based on camera rotation
            
            if (finalDirection.magnitude > ZeroF) {
                HandleRotation(finalDirection);
                HandleHorizontalMovement(finalDirection);
                SmoothSpeed(finalDirection.magnitude);
            } else {
                SmoothSpeed(ZeroF);
                
                rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
            }
        }

        void HandleHorizontalMovement(Vector3 adjustedDirection) {
            Vector3 velocity = adjustedDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        void HandleRotation(Vector3 adjustedDirection) {
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        void SmoothSpeed(float value) {
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, value, ref _velocity, smoothTime);
        }
    }
}