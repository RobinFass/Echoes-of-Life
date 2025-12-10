using System.Collections;
using UnityEngine;

namespace Common
{
    public class EnemyAttack : MonoBehaviour, IAttack
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
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (!(lastAttackTime < 0) || !projectilePrefab || hits is not { Length: > 0 }) return;
            animator.SetTrigger("Attack");
            StartCoroutine(ShootAt((hits[0].transform.position - transform.position).normalized));
            lastAttackTime = attackCooldown;
        }

        private IEnumerator ShootAt(Vector3 direction)
        {
            var spawnPos = transform.position;
            var projObj = Instantiate(projectilePrefab, spawnPos + direction, Quaternion.identity);
            var projComp = projObj.GetComponent<Projectile>();
            if (!projComp) yield break;
            projComp.StartCoroutine(projComp.MoveInDirection(direction, projectileSpeed));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
