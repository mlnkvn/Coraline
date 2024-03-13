using UnityEngine;

namespace Coraline {
    public class JumpState : BaseState {
        public JumpState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter() {
            animator.CrossFade(JumpHash, CrossFadeDuration);
        }

        public override void FixedUpdate() {
            player.HandleJump();
            player.HandleMovement();
        }
    }
}