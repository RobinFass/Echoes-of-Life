using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static int levelNumber = 1;
    
    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    public event EventHandler OnPlayerDeath;
    
    private int _score;
    public int Score => _score;
    private float health;
    private float maxHealth = 5f;
    private GameState state;
    public GameState State
    {
        get => state;
        set => state = value;
    }
    public float NormalizedHealth => health/maxHealth;
    
    private void Awake()
    {
        Instance = this;
        health = maxHealth;
        state = GameState.Playing;
    }
    
    private void Start()
    {
        Player.Instance.OnPickUpCoin += Player_OnPickUpCoin;
        Player.Instance.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        Player.Instance.OnEnemyHit += Player_OnEnemyHit;
        
        LoadCurrentLevel();
    }

    private void LoadCurrentLevel()
    {
        foreach (var level in levelList)
        {
            if (level.LevelNumber == levelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                Player.Instance.transform.position = spawnedLevel.StartPosition;
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());
                return;
            }
        }
        Debug.LogError("No level found for level number " + levelNumber);
    }

    private void Player_OnEnemyHit(object sender, EventArgs e)
    {
        health -= 1;
        if (health <= 0)
        {
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
            State = GameState.Dead;
        }
    }

    private void Player_OnPickUpCoin(object sender, EventArgs e)
    {
        _score += 1;
    }
    
    private void Player_OnChangingRoomSetCameraBounds(object sender, Room room)
    {
        var roomCameraBounds = room.GetCameraBounds().GetBounds();
        CineCamera.Instance.SetCameraBounds(roomCameraBounds);
        CineCamera.Instance.transform.position = Player.Instance.transform.position;
    }
    
    public void NextLevel()
    {
        levelNumber++;
        SceneManager.LoadScene(0);
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
    }
}
