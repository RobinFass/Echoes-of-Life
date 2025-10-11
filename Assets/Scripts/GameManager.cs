using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private Room StartRoom;
    
    private int score = 0;
    private float health;
    private float maxHealth = 5f;
    public int GetScore()
    {
        return score;
    }
    public float GetHealth()
    {
        return health;
    }
    public float GetHealthNormalized()
    {
        return health/maxHealth;
    }
    private void Awake()
    {
        Instance = this;
        health = maxHealth;
        Player_OnChangingRoom(null, StartRoom);
    }
    
    private void Start()
    {
        Player.Instance.OnPickUpCoin += Player_OnPickUpCoin;
        Player.Instance.OnChangingRoom += Player_OnChangingRoom;
        Player.Instance.OnEnemyHit += Player_OnEnemyHit;
    }

    private void Player_OnEnemyHit(object sender, EventArgs e)
    {
        health -= 1;
        Debug.Log("Health: " + health);
        if (health <= 0)
        {
            Debug.Log("Game Over!");
        }
    }

    private void Player_OnPickUpCoin(object sender, EventArgs e)
    {
        score += 1;
    }
    
    private void Player_OnChangingRoom(object sender, Room room)
    {
        var roomCameraBounds = room.GetCameraBounds().GetBounds();
        CineCamera.Instance.SetCameraBounds(roomCameraBounds);
        CineCamera.Instance.transform.position = Player.Instance.transform.position;
    }
}
