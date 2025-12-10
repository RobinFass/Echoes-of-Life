using System;
using System.Threading.Tasks;
using Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Common.Player
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }
    
        [SerializeField] private float maxHealth = 10f;
        [SerializeField] private float maxStamina = 20f;
        [SerializeField] private float staminaRegenRate = 2f;
        [SerializeField] private float staminaRegenCooldown = 2f;
        [SerializeField] private float hurtCooldown = 1f;

        // Dev mode : toggle player invincibility easily
        [FormerlySerializedAs("Invincible")] [SerializeField]
        public bool invincible;

        private float health;
        private bool isDying;
        private float stamina;
        private float staminaCooldownTime;

        public PlayerStats(bool isSprintHold)
        {
            IsSprintHold = isSprintHold;
        }

        private static global::Object.Player Player => global::Object.Player.Instance;
        private static PlayerAnimation Anime => PlayerAnimation.Instance;
        private static GameManager GameManager => GameManager.Instance;

        public float HurtCooldownTime { get; private set; }
        public float NormalizedHealth => health / maxHealth;
        public float NormalizedStamina => stamina / maxStamina;

        private bool IsSprintHold { get; }
        
        public event EventHandler<Enemy> OnDeath;

        private void Awake()
        {
            health = maxHealth;
            stamina = maxStamina;
            Instance = this;
        }

        private void Start()
        {
            Player.OnEnemyHit += Player_OnEnemyHit;
        }

        private void FixedUpdate()
        {
            if(GameManager.State != GameState.Playing) return;
            if (invincible)
            {
                stamina = maxStamina;
                staminaCooldownTime = 0f;
                HurtCooldownTime = 0f;
                return;
            }
            if (HurtCooldownTime >= 0) HurtCooldownTime -= Time.deltaTime;
            if (staminaCooldownTime >= 0)
                staminaCooldownTime -= Time.deltaTime;
            else
                stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
        }

        private async void Player_OnEnemyHit(object sender, Enemy e)
        {
            if (invincible) return;
            if (isDying || HurtCooldownTime > 0 || PlayerMovement.Instance.Dashing) return;
            if (health - e.Damage <= 0f)
            {
                isDying = true;
                health = 0f;
                Anime.PlayDead();
                await Task.Delay(1200);
                OnDeath?.Invoke(this, e);
                GameManager.State = GameState.Dead;
                return;
            }
            health -= e.Damage;
            e.Health -= Object.Player.Attack.BodyDamage;
            AudioManager.Instance?.PlaySfx("playerHit");
            Anime.PlayHurt();
            HurtCooldownTime = hurtCooldown;
        }
    

        public bool UseStamina(float amount)
        {
            if (invincible) return true;
            if (amount <= 0f|| health == 0) return false;
            if (!(stamina >= amount)) return false;
            if(IsSprintHold) return false;
            stamina -= amount;
            staminaCooldownTime = staminaRegenCooldown;
            return true;
        }

        public void Heal(float amount)
        {
            health = Mathf.Min(maxHealth, health + amount);
        }
    }
}
