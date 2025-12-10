using UnityEngine;

namespace Common.Player
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform attackPosition;
    
        public static PlayerAnimation Instance { get; private set; }
        private static GameInput Input => GameInput.Instance;
        private static GameManager GameManager => GameManager.Instance;

        private Vector3 initialAttackLocalPos;
        public bool FacingRight => !spriteRenderer || !spriteRenderer.flipX;

        private void Awake()
        {
            Instance = this;
            if (attackPosition) initialAttackLocalPos = attackPosition.localPosition;
        }

        private void FixedUpdate()
        {
            if(GameManager.State != GameState.Playing) return;
            var move = Input.OnMove();
            animator.SetBool("isMoving", move != Vector2.zero);
            if (!attackPosition) return;
            switch (move.x)
            {
                case > 0:
                {
                    spriteRenderer.flipX = false;
                    var pos = initialAttackLocalPos;
                    pos.x = Mathf.Abs(initialAttackLocalPos.x);
                    attackPosition.localPosition = pos;
                    break;
                }
                case < 0:
                {
                    spriteRenderer.flipX = true;
                    var pos = initialAttackLocalPos;
                    pos.x = -Mathf.Abs(initialAttackLocalPos.x);
                    attackPosition.localPosition = pos;
                    break;
                }
            }
        }

        public void PlaySprint()
        {
            animator.SetBool("isSprinting", true);
        }

        public void PauseSprint()
        {
            animator.SetBool("isSprinting", false);
        }

        public void PlayAttack()
        {
            animator.SetTrigger("Attack");
        }

        public void PlayDash()
        {
            animator.SetTrigger("Dash");
        }

        public void PlayHurt()
        {
            animator.SetTrigger("Hurt");
        }

        public void PlayDead()
        {
            animator.SetTrigger("Dead");
        }
    }
}
