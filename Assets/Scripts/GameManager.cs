using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int levelNumber = 0;

    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    private GameState state;
    public static GameManager Instance { get; private set; }

    public GameState State
    {
        get => state;
        set
        {
            state = value;
            Time.timeScale = state is GameState.Playing ? 1 : 0;
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
        if(levelList.Count <= 0) return; // gamemanager used in home scene too
        
        player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        gameInput.OnMEnuEvent += GameInput_OnGamePause;

        LoadCurrentLevel();
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnControlsRequested;
    public event EventHandler<Enemy> OnPlayerWin;
    

    public void LoadCurrentLevel()
    {
        foreach (var level in levelList)
            if (level.LevelNumber == levelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                Player.Instance.transform.position = spawnedLevel.StartPosition;
                //Player.Instance.ChangeSprite(levelNumber);
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());
                State = GameState.Playing;
                return;
            }

        SceneLoader.LoadScene(Scenes.HomeScene);
        Debug.Log("No more levels to load, returning to home scene");
        levelNumber = 0;
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

    public void RestartLevel()
    {
        SceneLoader.LoadScene(Scenes.GameScene);
        State = GameState.Playing;
    }

    public void RequestControls()
    {
        OnControlsRequested?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteBossRoom(Enemy enemy)
    {
        OnPlayerWin?.Invoke(this, enemy);
        State = GameState.Won;
    }

    private void FixedUpdate()
    {
        if(!MapUI.Instance) return;
        if (gameInput.OnShowMap())
        {
            MapUI.Instance.Show();
            return;
        }
        MapUI.Instance.Hide();
    }
}