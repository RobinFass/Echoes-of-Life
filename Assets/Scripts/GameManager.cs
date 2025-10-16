using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static int levelNumber = 1;
    
    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    public event EventHandler OnPlayerDeath;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    
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
    private GameInput _gameInput => GameInput.Instance;
    private Player _player => Player.Instance;
    
    public float NormalizedHealth => health/maxHealth;
    
    private void Awake()
    {
        Instance = this;
        health = maxHealth;
    }
    
    private void Start()
    {
        _player.OnPickUpCoin += Player_OnPickUpCoin;
        _player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        _player.OnEnemyHit += Player_OnEnemyHit;
        _gameInput.OnMEnuEvent += GameInput_OnGamePause;
        
        LoadCurrentLevel();
        state = GameState.Playing;
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
        SceneLoader.LoadScene(Scenes.HomeScene);
        Debug.Log("No more levels to load, returning to home scene");
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
    
    private void GameInput_OnGamePause(object sender, EventArgs e)
    {
        if (Time.timeScale.Equals(1))
        {
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        PauseUnpause();
    }
    
    public void NextLevel()
    {
        levelNumber++;
        SceneLoader.LoadScene(Scenes.GameScene);
    }
    
    public void RestartLevel()
    {
        SceneLoader.LoadScene(Scenes.GameScene);
    }

    public void PauseUnpause()
    {
        Time.timeScale = Time.timeScale.Equals(1) ? Time.timeScale = 0 : Time.timeScale = 1;
    }
}
