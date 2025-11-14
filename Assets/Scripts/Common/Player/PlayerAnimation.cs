// csharp

using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform attackPosition;
    
    private Vector3 initialAttackLocalPos;
    public static PlayerAnimation Instance { get; private set; }

    private GameInput input => GameInput.Instance;
    private GameManager GameManager => GameManager.Instance;

    public bool FacingRight => !spriteRenderer || !spriteRenderer.flipX;

    private void Awake()
    {
        Instance = this;
        if (attackPosition)
            initialAttackLocalPos = attackPosition.localPosition;
    }

    private void FixedUpdate()
    {
        if(GameManager.State != GameState.Playing) return;
        var move = input.OnMove();
        animator.SetBool("isMoving", move != Vector2.zero);

        if (!attackPosition) return;
        if (move.x > 0)
        {
            spriteRenderer.flipX = false;
            var pos = initialAttackLocalPos;
            pos.x = Mathf.Abs(initialAttackLocalPos.x);
            attackPosition.localPosition = pos;
        }
        else if (move.x < 0)
        {
            spriteRenderer.flipX = true;
            var pos = initialAttackLocalPos;
            pos.x = -Mathf.Abs(initialAttackLocalPos.x);
            attackPosition.localPosition = pos;
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