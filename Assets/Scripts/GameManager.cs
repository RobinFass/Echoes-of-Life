using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static int levelNumber = 1;

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
            Time.timeScale = state == GameState.Playing ? 1f : 0f;
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
        if (levelList.Count <= 0) return; // GameManager used in home scene too

        player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        gameInput.OnMEnuEvent += GameInput_OnGamePause;

        LoadCurrentLevel();

        state = GameState.Playing;
        Time.timeScale = 1f;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnControlsRequested;

    private void LoadCurrentLevel()
    {
        foreach (var level in levelList)
        {
            if (level.LevelNumber != levelNumber) continue;

            var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
            Player.Instance.transform.position = spawnedLevel.StartPosition;
            Player.Instance.ChangeSprite(levelNumber - 1);
            Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());

            AudioManager.Instance?.PlayLevelMusic(levelNumber);

            Debug.Log($"[GameManager] Loaded level {levelNumber}, requested music for level {levelNumber}");
            return;
        }

        Debug.Log("[GameManager] No more levels to load, returning to home scene");
        ReturnToMenu();
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
            PauseGame();
        else
            ResumeGame();
    }

    public void PauseGame()
    {
        if (state == GameState.Pause) return;
        state = GameState.Pause;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance?.PauseMusic();
        AudioManager.Instance?.StopLoopingSfx(); // stop walk/run when game is paused
    }

    public void ResumeGame()
    {
        if (state == GameState.Playing) return;
        state = GameState.Playing;
        Time.timeScale = 1f;
        OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        AudioManager.Instance?.ResumeMusic();
        AudioManager.Instance?.StopLoopingSfx(); // ensure no stale loop after resume
    }

    public void NextLevel()
    {
        levelNumber++;
        Debug.Log($"[GameManager] NextLevel, new levelNumber = {levelNumber}");
        AudioManager.Instance?.StopLoopingSfx(); // safety: clear movement loop on level change
        SceneLoader.LoadScene(Scenes.GameScene);
        state = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        AudioManager.Instance?.StopLoopingSfx(); // safety on restart
        SceneLoader.LoadScene(Scenes.GameScene);
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void ReturnToMenu()
    {
        levelNumber = 1;
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.StopLoopingSfx(); // safety when leaving the game
        SceneLoader.LoadScene(Scenes.HomeScene);
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RequestControls() => OnControlsRequested?.Invoke(this, EventArgs.Empty);
}
