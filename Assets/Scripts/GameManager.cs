using System;
using System.Collections.Generic;
using Common;
using Object;
using UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static int LevelNumber;
    
    private static GameInput GameInput => GameInput.Instance;
    private static Player Player => Player.Instance;

    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    private GameState state;
    private bool controlsOpen;
    
    public GameState State
    {
        get => state;
        set
        {
            state = value;
            Time.timeScale = state is GameState.Playing ? 1f : 0f;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(levelList.Count <= 0) return;
        Player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        GameInput.OnMEnuEvent += GameInput_OnGamePause;
        LoadCurrentLevel();
        state = GameState.Playing;
        Time.timeScale = 1f;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnControlsRequested;
    public event EventHandler<Enemy> OnPlayerWin;

    private void LoadCurrentLevel()
    {
        foreach (var level in levelList)
            if (level.LevelNumber == LevelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                Player.transform.position = spawnedLevel.StartPosition;
                //player.ChangeSprite(levelNumber);
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());
                State = GameState.Playing;
                AudioManager.Instance?.PlayLevelMusic(LevelNumber);
                Debug.Log($"[GameManager] Loaded level {LevelNumber}, requested music for level {LevelNumber}");
                return;
            }
        SceneLoader.LoadScene(Scenes.HomeScene);
        Debug.Log("No more levels to load, returning to home scene");
        LevelNumber = 0;
    }

    private static void Player_OnChangingRoomSetCameraBounds(object sender, Room room)
    {
        var roomCameraBounds = room.GetCameraBounds().GetBounds();
        CineCamera.Instance.SetCameraBounds(roomCameraBounds);
        CineCamera.Instance.transform.position = Player.Instance.transform.position;
    }

    private void GameInput_OnGamePause(object sender, EventArgs e)
    {
        if (state == GameState.Playing)
        {
            PauseGame();
        }
        else
        {
            if (controlsOpen)
            {
                OnGamePaused?.Invoke(this, EventArgs.Empty);
                return;
            }
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        if (state == GameState.Pause) return;
        state = GameState.Pause;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance?.PauseMusic();
        AudioManager.Instance?.StopLoopingSfx();
    }

    public void ResumeGame()
    {
        if (state == GameState.Playing) return;
        state = GameState.Playing;
        Time.timeScale = 1f;
        OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance?.ResumeMusic();
    }

    public void RestartLevel()
    {
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.StopLoopingSfx();
        SceneLoader.LoadScene(Scenes.GameScene);
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void ReturnToMenu()
    {
        LevelNumber = 1;
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.StopLoopingSfx();
        SceneLoader.LoadScene(Scenes.HomeScene);
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RequestControls()
    {
        controlsOpen = true;
        OnControlsRequested?.Invoke(this, EventArgs.Empty);
    }

    public void CloseControlsToPause()
    {
        controlsOpen = false;
        state = GameState.Pause;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteBossRoom(Enemy enemy)
    {
        AudioManager.Instance?.StopLoopingSfx();
        AudioManager.Instance?.StopMusic();
        if (enemy != null && enemy.IsFinalBoss)
        {
            AudioManager.Instance?.PlaySfx("majorSuccess");
        }
        else
        {
            AudioManager.Instance?.PlaySfx("success");
        }
        OnPlayerWin?.Invoke(this, enemy);
        State = GameState.Won;
    }

    private void FixedUpdate()
    {
        if(!MapUI.Instance) return;
        if (GameInput.OnShowMap())
        {
            MapUI.Instance.Show();
            return;
        }
        MapUI.Instance.Hide();
    }
}
