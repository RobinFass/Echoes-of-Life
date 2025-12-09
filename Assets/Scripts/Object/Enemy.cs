using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 4;
    [SerializeField] private float damage = 1;
    [SerializeField] private Canvas canva;
    [SerializeField] private GameObject dropItemPrefab;
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
            mult = GameManager.levelNumber/4 + 1;
        maxHealth = Random.Range((float)(maxHealth*mult *0.5), (float)(maxHealth*mult * 1.5));
        health = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ShowHealthBar();
        }
    }

    private void SelfDestruct()
    {
        if (isBoss)
        {
            gameManager.CompleteBossRoom(this);
        }
        float dropChance = GameManager.levelNumber switch
        {
            1 => 0.10f, // 10%
            2 => 0.075f, // 7.5%
            3 => 0.05f, // 5%
            _ => 0f
        };
        if (!isBoss && dropItemPrefab && Random.Range(0f, 1f) < dropChance)
        {
            Instantiate(dropItemPrefab, gameObject.transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public void ShowHealthBar()
    {
        if(canva) canva.gameObject.SetActive(true);
    }
}