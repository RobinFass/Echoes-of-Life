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

    public void SelfDestruct()
    {
        if (isBoss)
        {
            gameManager.CompleteBossRoom(this);
        }

        if (Random.Range(0f, 1f) < (0.3 - GameManager.levelNumber/10.0) && !isBoss && dropItemPrefab)
            Instantiate(dropItemPrefab, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void ShowHealthBar()
    {
        if(canva) canva.gameObject.SetActive(true);
    }
}