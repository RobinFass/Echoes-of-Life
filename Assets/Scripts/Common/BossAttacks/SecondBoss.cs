using System.Collections;
using UnityEngine;

namespace Common.BossAttacks
{
    public class SecondBoss : MonoBehaviour, Attack
    {
        [SerializeField] private float radius = 5f;
        public float Radius { get => radius; set => radius = value; }

        [Header("Spawning")]
        [SerializeField] private GameObject[] prefabs;
        [SerializeField] private int spawnCount = 10;
        [SerializeField] private SpriteRenderer ground;
        [SerializeField] private float spawnDelay = 0.05f;
        [SerializeField] private float prefabLifetime = 2f;

        [Header("Attack / Detection")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private Animator animator;

        private float lastAttackTime = 0f;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void FixedUpdate()
        {
            lastAttackTime -= Time.fixedDeltaTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (lastAttackTime <= 0f && prefabs != null && prefabs.Length > 0 && hits.Length > 0)
            {
                if (animator != null)
                    animator.SetTrigger("Attack");

                StartCoroutine(SpawnRoutine());
                lastAttackTime = attackCooldown;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            if (prefabs == null || prefabs.Length == 0)
                yield break;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 pos = GetRandomPointInRoom();
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
                Destroy(instance, prefabLifetime);
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

        // aide visuelle dans l'éditeur pour le radius
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
