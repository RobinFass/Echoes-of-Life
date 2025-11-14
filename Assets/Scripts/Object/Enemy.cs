using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 4;
    [SerializeField] private float damage = 1;
    [SerializeField] private Canvas canva;

    private float health;

    public float NormalizedHealth => health / maxHealth;

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
        maxHealth = Random.Range(maxHealth / 2, maxHealth * 2 + 1);
        health = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        ShowHealthBar();
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void ShowHealthBar()
    {
        canva.gameObject.SetActive(true);
    }
}