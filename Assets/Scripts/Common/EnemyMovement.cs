using UnityEngine;

namespace Common
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float radius = 8f;
        [SerializeField] private float speed = 1000;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private Rigidbody2D rigidBody;
        [SerializeField] private EnemyAttack enemyAttack;

        private Transform player;

        private void FixedUpdate()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (hits is { Length: > 0 })
            {
                player = hits[0].transform;
                if (enemyAttack && (player.position - transform.position).magnitude < enemyAttack.radius)
                {
                    Vector3 direction = (player.position - transform.position).normalized;
                    rigidBody.AddForce(-direction * (speed * Time.fixedDeltaTime));
                }
                else
                {
                    Vector3 direction = (player.position - transform.position).normalized;
                    rigidBody.AddForce(direction * (speed * Time.fixedDeltaTime));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}