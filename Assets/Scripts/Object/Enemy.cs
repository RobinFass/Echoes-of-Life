using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 4;
    [SerializeField] private float damage = 1;
    [SerializeField] private Canvas canva;
    [SerializeField] private bool isBoss = false;
    [SerializeField] private bool isFinalBoss = false;

    private GameManager gameManager => GameManager.Instance;
    
    private float health;

    public float NormalizedHealth => health / maxHealth;
    
    public bool IsFinalBoss => isFinalBoss;

    public float Health
    {
        get => health;
        set
        {
            health = value;
            if (health <= 0) SelfDestruct();
        }
    }

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    private void Start()
    {
        var mult = 1;
        if(!isBoss)
            mult = GameManager.levelNumber/2 + 1;
        maxHealth = Random.Range(maxHealth*mult / 2, maxHealth*mult * 2);
        health = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ShowHealthBar();
        }
    }

    public void SelfDestruct()
    {
        if (isBoss)
        {
            gameManager.CompleteBossRoom(this);
        }
        Destroy(gameObject);
    }

    public void ShowHealthBar()
    {
        if(canva) canva.gameObject.SetActive(true);
    }
}