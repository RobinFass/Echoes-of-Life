using System.Collections;
using UnityEngine;

namespace Common
{
    public class EnemyAttack : MonoBehaviour, Attack
    {
        [SerializeField] private float radius = 5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 5;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private Animator animator;

        private float lastAttackTime;
        public float Radius => radius;
        private void FixedUpdate()
        {
            lastAttackTime-= Time.fixedDeltaTime;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (lastAttackTime < 0 && projectilePrefab && hits is { Length: > 0 })
            {
                animator.SetTrigger("Attack");
                StartCoroutine(ShootAt((hits[0].transform.position - transform.position).normalized));
                lastAttackTime = attackCooldown;
            }
        }

        private IEnumerator ShootAt(Vector3 direction)
        {
            var spawnPos = transform.position;
            var proj = Instantiate(projectilePrefab, spawnPos+direction, Quaternion.identity);
            var rb = proj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = (Vector2)direction * (10 * projectileSpeed);
            var elapsed = 0f;
            while (elapsed < 1000)
            {
                if (rb) rb.linearVelocity = direction * projectileSpeed;
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            if (rb) rb.linearVelocity = Vector2.zero;

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
