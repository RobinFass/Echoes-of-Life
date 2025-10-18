using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeepColliderWorldRotation : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Vector3 worldOffset = Vector3.zero;

    void Reset()
    {
        if (transform.parent)
            target = transform.parent;
    }

    void LateUpdate()
    {
        if (!target) return;
        transform.position = target.position + worldOffset;
        transform.rotation = Quaternion.identity;
    }
}