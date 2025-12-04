namespace Object
{
    using UnityEngine;
    using UnityEngine.Events;

    public class DamageableRelay : MonoBehaviour
    {
        [System.Serializable] public class DamageEvent : UnityEvent<float> {}

        [Header("Relay to your existing health/damage method")]
        public DamageEvent OnDamage;

        // Called by enemies/projectiles when no IDamageable is present.
        public void ReceiveDamage(float amount, Vector2 hitPoint, Vector2 hitNormal, GameObject source)
        {
            OnDamage?.Invoke(amount); // Wire this to your current health script in the Inspector
        }
    }
}