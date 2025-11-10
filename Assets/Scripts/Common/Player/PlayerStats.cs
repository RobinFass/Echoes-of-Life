using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float maxStamina = 20f;
    [SerializeField] private float staminaRegenRate = 2f; 
    [SerializeField] private float staminaRegenCooldown = 1f;

    private float health;
    private float stamina;
    private float staminaCooldownTime = 0;
    
    public float NormalizedHealth => health / maxHealth;
    public float NormalizedStamina => stamina / maxStamina;

    public event EventHandler OnDeath;
    private Player player => Player.Instance;
    
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

    private void Player_OnEnemyHit(object sender, Enemy e)
    {
        if(health - e.Damage <= 0f)
        {
            health = 0f;
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            health -= e.Damage;
            e.Health -= player.Attack.BodyDamage;
        }
    }

    private void FixedUpdate()
    {
        if (staminaCooldownTime >= 0)
        {
            staminaCooldownTime -= Time.deltaTime;
        }
        else
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.deltaTime);
        }
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
