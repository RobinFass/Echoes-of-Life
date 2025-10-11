using UnityEngine;

public class Enemy : MonoBehaviour
{
    private int health = 1;
    public int Health
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
    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
