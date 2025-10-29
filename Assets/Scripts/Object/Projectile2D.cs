namespace Object
{
    // csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private bool destroyOnHit = true;

    private Rigidbody2D rb;
    private GameObject owner;
    private float expiry;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        expiry = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= expiry)
        {
            if (destroyOnHit) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

    public void Launch(Vector2 position, Vector2 direction, GameObject owner)
    {
        transform.position = position;
        transform.right = direction.normalized;
        this.owner = owner;
        expiry = Time.time + lifetime;
        rb.linearVelocity = direction.normalized * speed;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore owner
        if (owner != null && other.attachedRigidbody != null && other.attachedRigidbody.gameObject == owner)
            return;

        // Layer filter
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        var hitPoint = other.ClosestPoint(transform.position);
        var hitNormal = ((Vector2)transform.position - hitPoint).sqrMagnitude > 0f
            ? ((Vector2)transform.position - hitPoint).normalized
            : Vector2.up;

        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage, hitPoint, hitNormal, owner != null ? owner : gameObject);
        }
        else if (other.TryGetComponent<DamageableRelay>(out var relay))
        {
            relay.ReceiveDamage(damage, hitPoint, hitNormal, owner != null ? owner : gameObject);
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(float value) => damage = value;
    public void SetMask(LayerMask mask) => hitMask = mask;
}
}