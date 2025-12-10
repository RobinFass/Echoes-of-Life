using System.Collections;
using UnityEngine;

namespace Common.BossAttacks
{
    public class ThirdBoss : MonoBehaviour, IAttack
    {
        [Header("Detection")]
        [SerializeField] private float detectRadius = 5f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Laser (ray) settings")]
        [SerializeField] private GameObject[] segmentPrefabs;
        [SerializeField] private int segmentCount = 2;
        [SerializeField] private float segmentSpacing = 0.5f;
        [SerializeField] private float segmentLifetime = 1.5f;
        [SerializeField] private float betweenSegmentDelay = 0.03f;
        [SerializeField] private float betweenRayDelay = 0.02f;

        [Header("Grid")]
        [SerializeField] private float gridOffset = 1f;

        [Header("Behavior")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private Animator animator;
        private float lastAttackTime;
        public float Radius => detectRadius;

        private void FixedUpdate()
        {
            lastAttackTime -= Time.fixedDeltaTime;
            var hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, playerLayer.value);
            if (!(lastAttackTime <= 0f) || segmentPrefabs is not { Length: > 0 } || hits.Length <= 0) return;
            if (animator != null) animator.SetTrigger("Attack");
            StartCoroutine(FirePattern());
            lastAttackTime = attackCooldown;
        }

        private IEnumerator FirePattern()
        {
            var verticalDirs = new [] { Vector2.up, Vector2.down };
            var horizontalDirs = new [] { Vector2.right, Vector2.left };
            var leftPos = transform.position + Vector3.left * gridOffset;
            var rightPos = transform.position + Vector3.right * gridOffset;
            var upPos = transform.position + Vector3.up * gridOffset;
            var downPos = transform.position + Vector3.down * gridOffset;
            foreach (var dir in verticalDirs)
            {
                StartCoroutine(FireRayRoutine(dir, leftPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
                StartCoroutine(FireRayRoutine(dir, rightPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
            }
            foreach (var dir in horizontalDirs)
            {
                StartCoroutine(FireRayRoutine(dir, upPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
                StartCoroutine(FireRayRoutine(dir, downPos));
                if (betweenRayDelay > 0f) yield return new WaitForSeconds(betweenRayDelay);
            }
        }
        
        private IEnumerator FireRayRoutine(Vector2 direction, Vector3 origin)
        {
            for (int i = 1; i <= segmentCount; i++)
            {
                var spawnPos = origin + (Vector3)(direction * (i * segmentSpacing));
                float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float angleDeg = (baseAngle + 360f) % 360f;
                int prefabIndex = Mathf.RoundToInt(angleDeg / 90f) % 4;
                if (segmentPrefabs == null || segmentPrefabs.Length == 0) yield break;
                if (prefabIndex < 0 || prefabIndex >= segmentPrefabs.Length)
                    prefabIndex = Mathf.Clamp(prefabIndex, 0, segmentPrefabs.Length - 1);
                var prefabToSpawn = segmentPrefabs[prefabIndex];
                var seg = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                var s = seg.transform.localScale;
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                seg.transform.localScale = s;
                var rb = seg.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.freezeRotation = false;
                }
                Destroy(seg, segmentLifetime);
                yield return new WaitForSeconds(betweenSegmentDelay);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
            Gizmos.color = Color.cyan;
            float laserRange = segmentCount * segmentSpacing;
            var leftPos = transform.position + Vector3.left * gridOffset;
            var rightPos = transform.position + Vector3.right * gridOffset;
            var upPos = transform.position + Vector3.up * gridOffset;
            var downPos = transform.position + Vector3.down * gridOffset;
            Gizmos.DrawLine(leftPos + Vector3.up * laserRange, leftPos + Vector3.down * laserRange);
            Gizmos.DrawLine(rightPos + Vector3.up * laserRange, rightPos + Vector3.down * laserRange);
            Gizmos.DrawLine(upPos + Vector3.left * laserRange, upPos + Vector3.right * laserRange);
            Gizmos.DrawLine(downPos + Vector3.left * laserRange, downPos + Vector3.right * laserRange);
        }
    }
}
