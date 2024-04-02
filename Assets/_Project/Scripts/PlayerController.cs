using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField, Anywhere] public GameObject myLeftHand;
        [SerializeField, Anywhere] public GameObject myRightHand;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.2f;
        
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float jumpDuration = 0.4f;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float gravityMultiplier = 7f;

        [Header("Interact Settings")]
        [SerializeField] private List<GameObject> interactableObjects = new ();


        internal bool hasItemLeft;
        internal bool hasItemRight;
        
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
            
            var localRenderer = GetComponent<Renderer>();
            if (localRenderer)
            {
                localRenderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
            
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

        private void SetupTimers() {
            _jumpTimer = new CountdownTimer(jumpDuration);
            _jumpCooldownTimer = new CountdownTimer(jumpCooldown);

            _jumpTimer.OnTimerStart += () => _jumpVelocity = jumpForce;
            _jumpTimer.OnTimerStop += () => _jumpCooldownTimer.Start();

            _timers = new List<Timer>(2) {_jumpTimer, _jumpCooldownTimer};
        }

        private void At(IState from, IState to, IPredicate condition) => _stateMachine.AddTransition(from, to, condition);
        private void Any(IState to, IPredicate condition) => _stateMachine.AddAnyTransition(to, condition);

        private void Start()
        {
            input.EnablePlayerActions();
            hasItemLeft = false;
            hasItemRight = false;
        }

        private void OnEnable() {
            input.Jump += OnJump;
            input.ObjectInteract += OnObjectInteract;
        }
        
        private void OnDisable() {
            input.Jump -= OnJump;
            input.ObjectInteract -= OnObjectInteract;
        }

        private void OnJump(bool performed)
        {
            if (performed && !_jumpTimer.IsRunning && !_jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                _jumpTimer.Start();
            }
            else if (!performed && _jumpTimer.IsRunning)
            {
                _jumpTimer.Stop();
            }
        }

        private const float interactableDistance = 2f;

        private bool IsReachable(GameObject targetObject)
        {
            return Vector3.Distance(targetObject.transform.position, transform.position) <= interactableDistance;
        }

        internal bool HoldsObject(string objectTag)
        {
            return hasItemLeft && myLeftHand.transform.GetChild(0).CompareTag(objectTag) ||
                   hasItemRight && myRightHand.transform.GetChild(0).CompareTag(objectTag);
        }
        
        private void OnObjectInteract(bool performed)
        {
            if (!performed) return;
            foreach (var targetObject in interactableObjects.Where(IsReachable))
            {
                if (hasItemLeft && myLeftHand.transform.GetChild(0) == targetObject.transform ||
                    hasItemRight && myRightHand.transform.GetChild(0) == targetObject.transform)
                {
                    continue;
                }
                targetObject.GetComponent<InteractController>().Interact();
            }
        }

        private void Update() {
            _movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            _stateMachine.Update();

            HandleTimers();
            UpdateAnimator();
        }

        private void FixedUpdate() {
            _stateMachine.FixedUpdate();
        }

        private void UpdateAnimator() {
            animator.SetFloat(Speed, _currentSpeed);
        }

        private void HandleTimers() {
            foreach (var timer in _timers) {
                timer.Tick(Time.deltaTime);
            }
        }

        public void HandleJump() {
            if (!groundChecker.IsGrounded) return;
            switch (_jumpTimer.IsRunning)
            {
                case false when groundChecker.IsGrounded:
                    _jumpVelocity = ZeroF;
                    return;
                case false:
                    _jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
                    break;
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
            
            if (finalDirection.magnitude > ZeroF) {
                HandleRotation(finalDirection);
                HandleHorizontalMovement(finalDirection);
                SmoothSpeed(finalDirection.magnitude);
                
            } else {
                SmoothSpeed(ZeroF);
                rb.velocity = new Vector3(ZeroF, rb.velocity.y, ZeroF);
            }
        }

        private void HandleHorizontalMovement(Vector3 adjustedDirection) {
            var velocity = adjustedDirection * (moveSpeed * Time.fixedDeltaTime);
            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
        }

        private void HandleRotation(Vector3 adjustedDirection) {
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            freeLookVCam.transform.rotation = Quaternion.RotateTowards(freeLookVCam.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 5f);
        }

        private void SmoothSpeed(float value) {
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, value, ref _velocity, smoothTime);
        }
    }
}