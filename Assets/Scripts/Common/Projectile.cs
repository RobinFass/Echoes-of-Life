using System.Collections;
using UnityEngine;

namespace Common
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private LayerMask collisionLayers;

        private bool _hasCollided;

        public IEnumerator MoveInDirection(Vector2 direction, float speed)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (!rb) yield break;

            _hasCollided = false;

            while (!_hasCollided && gameObject)
            {
                rb.linearVelocity = direction * speed;
                yield return new WaitForFixedUpdate();
            }
            
            if (rb) rb.linearVelocity = Vector2.zero;
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if((collisionLayers.value & (1 << collision.gameObject.layer))!=0)
                Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            _hasCollided = true;
        }
    }
}