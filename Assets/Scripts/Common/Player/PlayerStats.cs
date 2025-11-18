using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float maxStamina = 20f;
    [SerializeField] private float staminaRegenRate = 2f;
    [SerializeField] private float staminaRegenCooldown = 1f;
    [SerializeField] private float hurtCooldown = 1f;

    private float health;
    private bool isDying;
    private float stamina;
    private float staminaCooldownTime;
    private float hurtCooldownTime;
    public static PlayerStats Instance { get; private set; }


    public float NormalizedHealth => health / maxHealth;
    public float NormalizedStamina => stamina / maxStamina;
    private Player player => Player.Instance;
    private PlayerAnimation anime => PlayerAnimation.Instance;
    private GameManager GameManager => GameManager.Instance;


    private void Awake()
    {
        health = maxHealth;
        stamina = maxStamina;
        Instance = this;
    }

    private void Start()
    {
        player.OnEnemyHit += Player_OnEnemyHit;
    }

    private void FixedUpdate()
    {
        if(GameManager.State != GameState.Playing) return;
        if (hurtCooldownTime >= 0)
            hurtCooldownTime -= Time.deltaTime;

        if (staminaCooldownTime >= 0)
            staminaCooldownTime -= Time.deltaTime;
        else
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
    }

    public event EventHandler<Enemy> OnDeath;

    private async void Player_OnEnemyHit(object sender, Enemy e)
    {
        if (isDying || hurtCooldownTime > 0) return;

        if (health - e.Damage <= 0f)
        {
            isDying = true;
            health = 0f;
            GameManager.State = GameState.Dead;
            anime.PlayDead();
            await Task.Delay(1200); // wait for death animation
            OnDeath?.Invoke(this, e);
            return;
        }

        health -= e.Damage;
        e.Health -= player.Attack.BodyDamage;
        anime.PlayHurt();
        hurtCooldownTime = hurtCooldown;
    }
    

    public bool UseStamina(float amount)
    {
        if (amount <= 0f) return true;
        if (stamina >= amount)
        {
            stamina -= amount;
            staminaCooldownTime = staminaRegenCooldown;
            return true;
        }

        return false;
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void RestoreStamina(float amount)
    {
        if (amount <= 0f) return;
        stamina = Mathf.Min(maxStamina, stamina + amount);
    }
}