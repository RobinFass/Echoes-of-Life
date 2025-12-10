using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int levelNumber = 0;

    [SerializeField] private LayerMask backgroundCollisionMask;
    [SerializeField] private List<GameLevel> levelList;

    private GameState state;
    private bool ControlsOpen = false;
    public static GameManager Instance { get; private set; }

    public GameState State
    {
        get => state;
        set
        {
            state = value;
            Time.timeScale = state is GameState.Playing ? 1f : 0f;
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
            if (level.LevelNumber == levelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                player.transform.position = spawnedLevel.StartPosition;
                //player.ChangeSprite(levelNumber);
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());
                State = GameState.Playing;
                AudioManager.Instance?.PlayLevelMusic(levelNumber);
                Debug.Log($"[GameManager] Loaded level {levelNumber}, requested music for level {levelNumber}");
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
        if (state == GameState.Playing)
        {
            PauseGame();
        }
        else
        {
            // If Controls are open and Escape is pressed, we want to go back to Pause menu,
            // not resume gameplay.
            if (ControlsOpen)
            {
                // Re-fire pause event (PauseUI will show). Do not change state (already Pause).
                OnGamePaused?.Invoke(this, EventArgs.Empty);
                return;
            }
            ResumeGame();
        }
    }

    public void PauseGame()
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
        levelNumber = 1;
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.StopLoopingSfx(); // ensure no loop when going back to menu
        SceneLoader.LoadScene(Scenes.HomeScene);
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RequestControls()
    {
        ControlsOpen = true;
        OnControlsRequested?.Invoke(this, EventArgs.Empty);
    }

    // Called when leaving Controls back to Pause
    public void CloseControlsToPause()
    {
        ControlsOpen = false;
        // Ensure pause state and event are consistent
        state = GameState.Pause;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void CompleteBossRoom(Enemy enemy)
    {
        // stop any ongoing loops/music when bossfight ends so NextLevelUI is silent
        AudioManager.Instance?.StopLoopingSfx();
        AudioManager.Instance?.StopMusic();

        // Play appropriate success SFX based on boss
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
        if (gameInput.OnShowMap())
        {
            MapUI.Instance.Show();
            return;
        }
        MapUI.Instance.Hide();
    }
}