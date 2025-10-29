// csharp
using System.Collections;
using Object;
using UnityEngine;

public class EnemyAttackController2D : MonoBehaviour
{
    [Header("Room (optional)")]
    [Tooltip("If set, attacks are active only when the player is inside this collider.")]
    [SerializeField] private Collider2D roomArea;

    [Header("Targeting")]
    [SerializeField] private LayerMask targetsMask;      // e.g., Player layer
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask obstacleMask = 0; // e.g., Walls/Environment

    [Header("Melee")]
    [SerializeField] private bool enableMelee = true;
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeRange = 1.2f;
    [SerializeField] private float meleeCooldown = 0.8f;
    [SerializeField] private Transform attackPoint; // if null, uses transform.position
    [SerializeField] private int maxMeleeHits = 4;

    [Header("Ranged")]
    [SerializeField] private bool enableRanged = false;
    [SerializeField] private float rangedDamage = 8f;
    [SerializeField] private float rangedCooldown = 1.5f;
    [SerializeField] private float rangedMinDistance = 2.0f;
    [SerializeField] private float rangedMaxDistance = 8.0f;
    [SerializeField] private Transform shootPoint; // spawn origin
    [SerializeField] private Projectile2D projectilePrefab;

    private Transform target; // Player
    private bool ready;
    private bool playerInRoom;

    private float nextMeleeTime;
    private float nextRangedTime;

    private readonly Collider2D[] meleeHitsBuffer = new Collider2D[16];

    private void OnEnable()
    {
        ready = false;
        StartCoroutine(ResolvePlayerOnce());
    }

    private IEnumerator ResolvePlayerOnce()
    {
        // Wait for Player singleton to exist
        while (Player.Instance == null)
            yield return null;

        target = Player.Instance.transform;
        ready = true;
    }

    private void Update()
    {
        if (!ready || target == null)
            return;

        // Room gating
        playerInRoom = roomArea == null || roomArea.OverlapPoint(target.position);
        if (!playerInRoom)
            return;

        float dist = Vector2.Distance(transform.position, target.position);
        bool canSee = !requireLineOfSight || HasLineOfSight();

        // Melee first if in range
        if (enableMelee && Time.time >= nextMeleeTime && dist <= meleeRange && canSee)
        {
            DoMelee();
            nextMeleeTime = Time.time + meleeCooldown;
            return; // prioritize melee over ranged in same frame
        }

        // Ranged if in band
        if (enableRanged && projectilePrefab != null && shootPoint != null && Time.time >= nextRangedTime)
        {
            if (dist >= rangedMinDistance && dist <= rangedMaxDistance && canSee)
            {
                DoRanged();
                nextRangedTime = Time.time + rangedCooldown;
            }
        }
    }

    private void DoMelee()
    {
        Vector2 origin = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        int count = Physics2D.OverlapCircleNonAlloc(origin, meleeRange, meleeHitsBuffer, targetsMask);

        for (int i = 0; i < count; i++)
        {
            var c = meleeHitsBuffer[i];
            if (c == null) continue;

            Vector2 hitPoint = c.ClosestPoint(origin);
            Vector2 hitNormal = (hitPoint - origin).sqrMagnitude > 0f ? (hitPoint - origin).normalized : Vector2.up;

            if (c.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(meleeDamage, hitPoint, hitNormal, gameObject);
            else if (c.TryGetComponent<DamageableRelay>(out var relay))
                relay.ReceiveDamage(meleeDamage, hitPoint, hitNormal, gameObject);

            meleeHitsBuffer[i] = null; // clear slot
        }
    }

    private void DoRanged()
    {
        Vector2 from = (shootPoint != null ? (Vector2)shootPoint.position : (Vector2)transform.position);
        Vector2 to = (Vector2)target.position;
        Vector2 dir = (to - from).sqrMagnitude > 0f ? (to - from).normalized : Vector2.right;

        var proj = Instantiate(projectilePrefab);
        proj.SetDamage(rangedDamage);
        proj.SetMask(targetsMask);
        proj.Launch(from, dir, gameObject);
    }

    private bool HasLineOfSight()
    {
        if (target == null) return false;
        Vector2 from = transform.position;
        Vector2 to = target.position;
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        // Melee range
        Gizmos.color = Color.magenta;
        Vector3 p = attackPoint != null ? attackPoint.position : transform.position;
        Gizmos.DrawWireSphere(p, meleeRange);

        // Ranged band
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, rangedMinDistance);
        Gizmos.DrawWireSphere(transform.position, rangedMaxDistance);
    }
}
