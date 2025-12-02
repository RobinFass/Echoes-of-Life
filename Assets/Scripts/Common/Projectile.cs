using UnityEngine;

namespace Common
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private LayerMask collisionLayers;
        
        public void OnCollisionStay2D(Collision2D collision)
        {
            if((collisionLayers.value & (1 << collision.gameObject.layer))!=0)
                Destroy(gameObject);
        }
    }
}