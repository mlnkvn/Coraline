using UnityEngine;

namespace Coraline {
    public abstract class BaseState : IState {
        protected readonly PlayerController player;
        protected readonly Animator animator;
        
        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        
        protected const float CrossFadeDuration = 0.1f;
        
        protected BaseState(PlayerController player, Animator animator) {
            this.player = player;
            this.animator = animator;
        }
        
        public virtual void OnEnter() {
        }

        public virtual void Update() {
        }

        public virtual void FixedUpdate() {
        }

        public virtual void OnExit() {
        }
    }
}