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
                Vector3 pos = GetRandomPointInRoom();
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

        private Vector3 GetRandomPointInRoom()
        {
            if (ground != null)
            {
                Bounds b = ground.bounds;
                float x = Random.Range(b.min.x, b.max.x);
                float y = Random.Range(b.min.y, b.max.y);
                float z = Random.Range(b.min.z, b.max.z);
                return new Vector3(x, y, z);
            }

            Vector2 rand = Random.insideUnitCircle * radius;
            return transform.position + new Vector3(rand.x, 0f, rand.y);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
