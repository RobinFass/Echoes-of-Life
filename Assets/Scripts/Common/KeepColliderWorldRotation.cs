using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(Collider2D))]
    public class KeepColliderWorldRotation : MonoBehaviour
    {
        [SerializeField] private Transform target;
        
        private readonly Vector3 worldOffset = Vector3.zero;

        private void Reset()
        {
            if (transform.parent) target = transform.parent;
        }

        private void LateUpdate()
        {
            if (!target) return;
            transform.position = target.position + worldOffset;
            transform.rotation = Quaternion.identity;
        }
    }
}
