using System.Collections;
using UnityEngine;

namespace Common.BossAttacks
{
    public class FirstBoss : MonoBehaviour, IAttack
    {
        [Header("Général")]
        [SerializeField] private float detectionRadius = 6f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private Animator animator;
        [SerializeField] private float attackCooldown = 2f;

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 6f;

        [Header("Ring Attack")]
        [SerializeField] private int ringCount = 10;
        [SerializeField] private float ringSpawnRadius = 1.2f;

        [Header("Spread Attack")]
        [SerializeField] private int spreadCount = 3;
        [SerializeField] private float spreadAngle = 30f;

        private float lastAttackTime;
        private bool nextIsRing = true;
        public float Radius => detectionRadius;

        private void FixedUpdate()
        {
            lastAttackTime -= Time.fixedDeltaTime;
            var hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
            if (!(lastAttackTime <= 0f) || hits is not { Length: > 0 }) return;
            var playerPos = hits[0].transform.position;
            var dir = (playerPos - transform.position).normalized;
            nextIsRing = Random.value > 0.5;
            StartCoroutine(nextIsRing ? RingAttack() : SpreadAttack(dir));
            lastAttackTime = attackCooldown;
        }

        private IEnumerator ShootAt(Vector2 direction, Vector3 spawnPos)
        {
            if (projectilePrefab == null) yield break;
            var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var rb = proj.GetComponent<Rigidbody2D>();
            float elapsed = 0f;
            while (elapsed < 1000f)
            {
                if (rb) rb.linearVelocity = direction * projectileSpeed;
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            if (rb) rb.linearVelocity = Vector2.zero;
        }

        private IEnumerator RingAttack()
        {
            if (projectilePrefab == null) yield break;
            float angleStep = 360f / Mathf.Max(1, ringCount);
            for (int i = 0; i < ringCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var spawnPos = transform.position + (Vector3)(dir * ringSpawnRadius);
                StartCoroutine(ShootAt(dir, spawnPos));
            }
            yield return null;
        }

        private IEnumerator SpreadAttack(Vector2 direction)
        {
            if (projectilePrefab == null) yield break;
            if (direction == Vector2.zero) direction = Vector2.right;
            float half = spreadAngle / 2f;
            float step = spreadCount > 1 ? (spreadAngle / (spreadCount - 1)) : 0f;
            for (int i = 0; i < spreadCount; i++)
            {
                float angleOffset = -half + step * i;
                float rad = angleOffset * Mathf.Deg2Rad;
                var dir = new Vector2(
                    direction.x * Mathf.Cos(rad) - direction.y * Mathf.Sin(rad),
                    direction.x * Mathf.Sin(rad) + direction.y * Mathf.Cos(rad)
                ).normalized;
                var spawnPos = transform.position + (Vector3)(dir * ringSpawnRadius);
                StartCoroutine(ShootAt(dir, spawnPos));
            }
            yield return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
