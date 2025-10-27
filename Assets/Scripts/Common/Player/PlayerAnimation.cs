// csharp
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public static PlayerAnimation Instance { get; private set; }

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform attackPosition;

    private GameInput input => GameInput.Instance;
    private Vector3 initialAttackLocalPos;
    private CapsuleCollider2D capsule;

    private void Awake()
    {
        Instance = this;
        if (attackPosition != null)
            initialAttackLocalPos = attackPosition.localPosition;
        capsule = Player.Instance.GetComponent<CapsuleCollider2D>();
        
    }

    public bool FacingRight => !spriteRenderer || !spriteRenderer.flipX;

    private void FixedUpdate()
    {
        var move = input.OnMove();
        animator.SetBool("isMoving", move != Vector2.zero);

        if (!attackPosition) return;
        if (move.x > 0)
        {
            spriteRenderer.flipX = false;
            var pos = initialAttackLocalPos;
            pos.x = Mathf.Abs(initialAttackLocalPos.x);
            attackPosition.localPosition = pos;
            capsule.offset = new Vector2(1,-1);
        }
        else if (move.x < 0)
        {
            spriteRenderer.flipX = true;
            var pos = initialAttackLocalPos;
            pos.x = -Mathf.Abs(initialAttackLocalPos.x);
            attackPosition.localPosition = pos;
            capsule.offset = new Vector2(-1,-1);
            
        }
    }

    public void PlayAttack()
    {
        Debug.Log("Oni Giri !");
        // animator.SetTrigger("Attack");
    }
}