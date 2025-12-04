using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform attackPos;

    [Header("Layers")] 
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private float attackDamage = 2f;
    [SerializeField] private float bodyDamage = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float coneAngle = 140f;

    [Header("Orientation")]
    [SerializeField] private float orientationAngle;

    [Header("Knockback (knockBack)")]
    [SerializeField] private float knockbackDistance = 2f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("Raycast Cone")]
    [SerializeField] private float rayStepDeg = 5f;

    private float attackCooldownTimer;
    public static PlayerAttack Instance { get; private set; }
    private GameInput input => GameInput.Instance;
    private GameManager GameManager => GameManager.Instance;

    private PlayerAnimation playerAnimation => Player.Instance ? Player.Instance.Animation : null;

    // expose an event so other systems (like audio) can react to attacks
    public event EventHandler OnAttack;
    private static bool IsInLayer(LayerMask mask, int layer) => (mask.value & (1 << layer)) != 0;

    public float BodyDamage { get => bodyDamage; set => bodyDamage = value; }

    private void Awake() => Instance = this;

    private void Start()
    {
        if (input) input.OnAttackEvent += OnAttackEvent;
    }

    private void FixedUpdate()
    {
        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    private Vector2 GetConeCenterDirection()
    {
        if (!attackPos) return Vector2.right;
        var baseDir = attackPos.right;
        var rotated = Quaternion.Euler(0f, 0f, orientationAngle) * baseDir;
        var dir2 = new Vector2(rotated.x, rotated.y).normalized;
        var facingRight = playerAnimation ? playerAnimation.FacingRight :
            (Player.Instance && Player.Instance.Animation ? Player.Instance.Animation.FacingRight : true);
        if (!facingRight) dir2 = new Vector2(-dir2.x, dir2.y);
        return dir2;
    }

    private List<Collider2D> CastConeRays(Vector2 origin, Vector2 coneDir, float halfAngleDeg, float range,
        LayerMask enemyMask, LayerMask obstacleMask, float stepDeg)
    {
        var found = new HashSet<Collider2D>();
        var combined = enemyMask | obstacleMask; // correction

        var steps = Mathf.Max(1, Mathf.CeilToInt((halfAngleDeg * 2f) / Mathf.Max(0.1f, stepDeg)));
        const float eps = 0.01f;
        for (int i = 0; i <= steps; i++)
        {
            float a = -halfAngleDeg + (i * (halfAngleDeg * 2f) / steps);
            var dir3 = Quaternion.Euler(0f, 0f, a) * new Vector3(coneDir.x, coneDir.y, 0f);
            var dir = new Vector2(dir3.x, dir3.y).normalized;
            var hit = Physics2D.Raycast(origin + dir * eps, dir, Mathf.Max(0f, range - eps), combined);
            if (hit.collider && IsInLayer(enemyMask, hit.collider.gameObject.layer))
                found.Add(hit.collider);
        }
        return new List<Collider2D>(found);
    }

    private void OnAttackEvent(object sender, EventArgs e)
    {
        if(GameManager.State != GameState.Playing) return;
        if (attackCooldownTimer > 0f) return;
        if (!attackPos) return;

        // attack is accepted: notify listeners (Player will play "sword" SFX)
        OnAttack?.Invoke(this, EventArgs.Empty);

        playerAnimation.PlayAttack();
        attackCooldownTimer = attackCooldown;

        var origin = (Vector2)attackPos.position;
        var coneDir = GetConeCenterDirection();
        var half = coneAngle * 0.5f;
        var hitEnemies = CastConeRays(origin, coneDir, half, attackRange, enemyLayer, obstacleLayer, rayStepDeg);

        foreach (var enemyCollider in hitEnemies)
        {
            if (!enemyCollider) continue;
            var enemy = enemyCollider.GetComponent<Enemy>();
            if (!enemy) continue;

            // successful hit: play monster-hit SFX
            AudioManager.Instance?.PlaySfx("monsterHit");
            
            enemy.ShowHealthBar();
            enemy.Health -= attackDamage;
            if (enemy.Health <= 0) continue;

            var hitPoint = enemyCollider.ClosestPoint(origin);
            var kbDir = (hitPoint - origin).sqrMagnitude > 1e-6f ? (hitPoint - origin).normalized : coneDir;
            var rb = enemy.GetComponent<Rigidbody2D>();
            StartCoroutine(KnockbackRigidbodyCoroutine(rb, kbDir));
        }
    }

    private IEnumerator KnockbackRigidbodyCoroutine(Rigidbody2D rb, Vector2 direction)
    {
        var elapsed = 0f;
        var kbSpeed = knockbackDistance / knockbackDuration;
        while (elapsed < knockbackDuration)
        {
            if (rb) rb.linearVelocity = direction * kbSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmos() // dessine même non sélectionné (Scene view avec Gizmos actif)
    {
        DrawConeGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawConeGizmos(true);
    }

    private void DrawConeGizmos(bool selected)
    {
        if (!attackPos) return;
        var origin3 = attackPos.position;
        var dir2 = GetConeCenterDirection();
        var dir3 = new Vector3(dir2.x, dir2.y, 0f);
        var half = coneAngle * 0.5f;

        var leftDir = Quaternion.Euler(0, 0, half) * dir3;
        var rightDir = Quaternion.Euler(0, 0, -half) * dir3;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin3, origin3 + leftDir * attackRange);
        Gizmos.DrawLine(origin3, origin3 + rightDir * attackRange);

        var segments = 32;
        var prev = origin3 + leftDir * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float a = Mathf.Lerp(half, -half, t);
            var segDir = Quaternion.Euler(0, 0, a) * dir3;
            var next = origin3 + segDir * attackRange;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        #if UNITY_EDITOR
        Handles.color = new Color(1f, 0f, 0f, 0.12f);
        Handles.DrawSolidArc(origin3, Vector3.forward, leftDir.normalized, -coneAngle, attackRange);

        var origin = (Vector2)origin3;
        var combined = enemyLayer | obstacleLayer;
        var steps = Mathf.Max(1, Mathf.CeilToInt((half * 2f) / Mathf.Max(0.1f, rayStepDeg)));
        const float eps = 0.01f;

        for (int i = 0; i <= steps; i++)
        {
            float a = -half + (i * (half * 2f) / steps);
            var d3 = Quaternion.Euler(0f, 0f, a) * dir3;
            var d2 = new Vector2(d3.x, d3.y).normalized;
            var hit = Physics2D.Raycast(origin + d2 * eps, d2, Mathf.Max(0f, attackRange - eps), combined);
            if (hit.collider)
            {
                Gizmos.color = IsInLayer(enemyLayer, hit.collider.gameObject.layer) ? Color.green : Color.yellow;
                Gizmos.DrawLine(origin3, hit.point);
                Gizmos.DrawSphere(hit.point, 0.05f);
            }
            else
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(origin3, origin3 + (Vector3)d2 * attackRange);
            }
        }
        #endif
    }
}