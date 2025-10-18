using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float maxHealth = 4;
    [SerializeField] private float damage = 1;
    [SerializeField] private Canvas canva;
    
    private float health = 4;
    
    public float NormalizedHealth =>  health / maxHealth;
    public float Health
    {
        get => health;
        set
        {
            health = value;
            if (health <= 0)
            {
                SelfDestruct();
            }
        }
    }
    
    public float Damage
    {
        get => damage;
        set => damage = value;
    }
    
    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        canva.gameObject.SetActive(true);
    }
    
    public void ShowHealthBar()
    {
        canva.gameObject.SetActive(true);
    }

}
