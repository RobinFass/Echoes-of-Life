using System.Collections;
using UnityEngine;

namespace Common.BossAttacks
{
    public class SecondBoss : MonoBehaviour, Attack
    {
        [SerializeField] private float radius = 5f;
        [SerializeField] private float attackRadius = 3f;
        public float Radius => 0;

        [Header("Spawning")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int spawnCount = 10;
        [SerializeField] private SpriteRenderer ground;
        [SerializeField] private float spawnDelay = 0.05f;
        [SerializeField] private float prefabLifetime = 2f;
        [SerializeField] private float crackRadius = 3f; // nouveau: rayon pour les cracks

        [Header("Indicator")]
        [SerializeField] private GameObject indicatorPrefab;
        [SerializeField] private float indicatorDuration = 0.5f;

        [Header("Attack / Detection")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private Animator animator;

        private float lastAttackTime = 0f;

        private void FixedUpdate()
        {
            lastAttackTime -= Time.fixedDeltaTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (lastAttackTime <= 0f && prefab != null && hits.Length > 0)
            {
                if (animator != null)
                    animator.SetTrigger("Attack");

                StartCoroutine(SpawnRoutine());
                lastAttackTime = attackCooldown;

                var toHit = Physics2D.OverlapCircleAll(transform.position, attackRadius, playerLayer.value);
                if (toHit.Length > 0)
                    Player.Instance.BeingHit(gameObject.GetComponent<Enemy>());
            }
        }

        private IEnumerator SpawnRoutine()
        {
            if (prefab == null)
                yield break;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 pos = GetRandomPointInRoom(crackRadius);
                if (indicatorPrefab != null)
                {
                    GameObject indicator = Instantiate(indicatorPrefab, pos, Quaternion.identity);
                    Destroy(indicator, indicatorDuration);
                    yield return new WaitForSeconds(indicatorDuration);
                }
                else
                {
                    // fallback: keep original small delay before spawn if no indicator assigned
                    yield return new WaitForSeconds(spawnDelay);
                }

                GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
                Destroy(instance, prefabLifetime);

                // spacing between successive spawns
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        private Vector3 GetRandomPointInRoom(float maxRadius)
        {
            if (ground != null)
            {
                Bounds b = ground.bounds;
                // essayer plusieurs fois de générer un point dans le cercle et à l'intérieur des bounds
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    Vector2 rand = Random.insideUnitCircle * maxRadius;
                    Vector3 candidate = transform.position + new Vector3(rand.x, rand.y, 0f);
                    if (candidate.x >= b.min.x && candidate.x <= b.max.x && candidate.y >= b.min.y && candidate.y <= b.max.y)
                        return candidate;
                }

                // fallback: clamp la position aux bounds (peut légèrement sortir du cercle si aucun point valide trouvé)
                float clampedX = Mathf.Clamp(transform.position.x, b.min.x, b.max.x);
                float clampedY = Mathf.Clamp(transform.position.y, b.min.y, b.max.y);
                return new Vector3(clampedX, clampedY, transform.position.z);
            }

            Vector2 randCircle = Random.insideUnitCircle * maxRadius;
            return transform.position + new Vector3(randCircle.x, randCircle.y, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, crackRadius); // affiche le rayon des cracks
        }
    }
}
