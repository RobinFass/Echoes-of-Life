using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPos;

    [Header("Layers")] [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack Settings")] [SerializeField]
    private float attackCooldown = 0.3f;

    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float bodyDamage = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float coneAngle = 140f;

    [Header("Orientation")] [SerializeField]
    private float orientationAngle;

    [Header("Knockback (knockBack)")] [SerializeField]
    private float knockbackDistance = 2f;

    [SerializeField] private float knockbackDuration = 0.15f;

    private float attackCooldownTimer;
    public static PlayerAttack Instance { get; private set; }
    private GameInput input => GameInput.Instance;
    private GameManager GameManager => GameManager.Instance;

    private PlayerAnimation playerAnimation => Player.Instance ? Player.Instance.Animation : null;

    public float BodyDamage
    {
        get => bodyDamage;
        set => bodyDamage = value;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (input != null)
            input.OnAttackEvent += OnAttackEvent;
    }

    private void FixedUpdate()
    {
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (!attackPos) return;

        var centerPos = attackPos.position;
        var centerDir2 = GetConeCenterDirection();
        var centerDir = new Vector3(centerDir2.x, centerDir2.y, 0f).normalized;

        var half = coneAngle * 0.5f;
        var leftDir = Quaternion.Euler(0, 0, half) * centerDir;
        var rightDir = Quaternion.Euler(0, 0, -half) * centerDir;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(centerPos, centerPos + leftDir * attackRange);
        Gizmos.DrawLine(centerPos, centerPos + rightDir * attackRange);

        var segments = 32;
        var prev = centerPos + leftDir * attackRange;
        for (var i = 1; i <= segments; i++)
        {
            var t = (float)i / segments;
            var angle = Mathf.Lerp(half, -half, t);
            var dir = Quaternion.Euler(0, 0, angle) * centerDir;
            var next = centerPos + dir * attackRange;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

#if UNITY_EDITOR
        Handles.color = new Color(1f, 0f, 0f, 0.12f);
        var from = new Vector3(leftDir.x, leftDir.y, 0f).normalized;
        Handles.DrawSolidArc(centerPos, Vector3.forward, from, -coneAngle, attackRange);
#endif
    }

    private Vector2 GetConeCenterDirection()
    {
        if (!attackPos) return Vector2.right;

        var baseDir = attackPos.right;

        var rotated = Quaternion.Euler(0f, 0f, orientationAngle) * baseDir;
        var dir2 = new Vector2(rotated.x, rotated.y).normalized;

        var facingRight = true;
        if (playerAnimation != null)
            facingRight = playerAnimation.FacingRight;
        else if (Player.Instance != null && Player.Instance.Animation != null)
            facingRight = Player.Instance.Animation.FacingRight;

        if (!facingRight)
            dir2 = new Vector2(-dir2.x, dir2.y);

        return dir2;
    }

    private void OnAttackEvent(object sender, EventArgs e)
    {
        if(GameManager.State != GameState.Playing) return;
        if (attackCooldownTimer > 0f) return;
        if (!attackPos) return;

        playerAnimation.PlayAttack();
        attackCooldownTimer = attackCooldown;

        var hitEnemiesCollider = Physics2D.OverlapCircleAll(
            attackPos.position, attackRange, enemyLayer);
        var coneCenterDir = GetConeCenterDirection();
        var halfAngle = coneAngle * 0.5f;

        foreach (var enemyCollider in hitEnemiesCollider)
        {
            if (!enemyCollider) continue;
            var enemy = enemyCollider.GetComponent<Enemy>();
            if (!enemy) continue;

            var toEnemy = (enemy.transform.position - attackPos.position).normalized;
            var angle = Vector2.Angle(coneCenterDir, toEnemy);
            if (angle > halfAngle) continue;

            enemy.ShowHealthBar();
            enemy.Health -= attackDamage;
            if (enemy.Health <= 0) continue;

            var rawDir = enemy.transform.position - attackPos.position;
            if (rawDir == Vector3.zero) continue;
            var dir = new Vector2(rawDir.normalized.x, rawDir.normalized.y);

            var rb = enemy.GetComponent<Rigidbody2D>();
            StartCoroutine(KnockbackRigidbodyCoroutine(rb, dir));
        }
    }

    private IEnumerator KnockbackRigidbodyCoroutine(Rigidbody2D rb, Vector2 direction)
    {
        var elapsed = 0f;
        var kbSpeed = knockbackDistance / knockbackDuration;
        while (elapsed < knockbackDuration)
        {
            if (rb)
                rb.linearVelocity = direction * kbSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (rb)
            rb.linearVelocity = Vector2.zero;
    }
}