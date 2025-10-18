using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static int levelNumber = 1;
    
    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    
    private int _score;
    public int Score => _score;
    private GameState state;
    public GameState State
    {
        get => state;
        set => state = value;
    }
    private GameInput gameInput => GameInput.Instance;
    private Player player => Player.Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        player.OnPickUpCoin += Player_OnPickUpCoin;
        player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        player.Stats.OnDeath += Player_OnDeath;
        gameInput.OnMEnuEvent += GameInput_OnGamePause;
        
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
    
    private void Player_OnDeath(object sender, EventArgs e)
    {
        state = GameState.Dead;
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
