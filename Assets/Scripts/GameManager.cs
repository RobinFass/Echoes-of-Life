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
        set
        {
            state = value;
            if(state == GameState.Playing)
            {
                Time.timeScale = 1;

            }
            else
            {
                Time.timeScale = 0;
            }
        }
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
        State = GameState.Playing;
    }

    private void LoadCurrentLevel()
    {
        foreach (var level in levelList)
        {
            if (level.LevelNumber == levelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                Player.Instance.transform.position = spawnedLevel.StartPosition;
                Player.Instance.ChangeSprite(levelNumber-1);
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());
                return;
            }
        }
        SceneLoader.LoadScene(Scenes.HomeScene);
        Debug.Log("No more levels to load, returning to home scene");
        levelNumber = 1;
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
            State = GameState.Pause;
        }
        else
        {
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
            State = GameState.Playing;
        }
    }
    
    private void Player_OnDeath(object sender, EventArgs e)
    {
        State = GameState.Dead;
    }
    
    public void NextLevel()
    {
        levelNumber++;
        SceneLoader.LoadScene(Scenes.GameScene);
        State = GameState.Playing;
    }
    
    public void RestartLevel()
    {
        SceneLoader.LoadScene(Scenes.GameScene);
        State = GameState.Playing;
    }
}
