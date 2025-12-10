using Unity.VisualScripting;
using UnityEngine;

namespace Common
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float radius = 8f;
        [SerializeField] private float speed = 1000;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private Rigidbody2D rigidBody;
        
        private IAttack enemyAttack;
        private Transform player;

        private void FixedUpdate()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer.value);
            if (hits is not { Length: > 0 }) return;
            player = hits[0].transform;
            if (!enemyAttack.IsUnityNull() && (player.position - transform.position).magnitude < enemyAttack.Radius)
            {
                var direction = (player.position - transform.position).normalized;
                rigidBody.AddForce(-direction * (speed * Time.fixedDeltaTime));
            }
            else
            {
                var direction = (player.position - transform.position).normalized;
                rigidBody.AddForce(direction * (speed * Time.fixedDeltaTime));
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
