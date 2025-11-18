using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int levelNumber = 1;

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

    private void Awake() => Instance = this;

    private void Start()
    {
        if (levelList.Count <= 0) return; // gamemanager used in home scene too
        player.OnChangingRoom += Player_OnChangingRoomSetCameraBounds;
        gameInput.OnMEnuEvent += GameInput_OnGamePause;

        LoadCurrentLevel();
        State = GameState.Playing;
    }

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnControlsRequested;

    private void LoadCurrentLevel()
    {
        foreach (var level in levelList)
            if (level.LevelNumber == levelNumber)
            {
                var spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
                Player.Instance.transform.position = spawnedLevel.StartPosition;
                Player.Instance.ChangeSprite(levelNumber - 1);
                Player_OnChangingRoomSetCameraBounds(null, spawnedLevel.GetStartRoom());

                AudioManager.Instance?.PlayLevelMusic(levelNumber);
                return;
            }

        ReturnToMenu();
        Debug.Log("No more levels to load, returning to home scene");
    }

    private void Player_OnChangingRoomSetCameraBounds(object sender, Room room)
    {
        var roomCameraBounds = room.GetCameraBounds().GetBounds();
        CineCamera.Instance.SetCameraBounds(roomCameraBounds);
        CineCamera.Instance.transform.position = Player.Instance.transform.position;
    }

    private void GameInput_OnGamePause(object sender, EventArgs e)
    {
        if (Time.timeScale.Equals(1f))
        {
            OnGamePaused?.Invoke(this, EventArgs.Empty);
            State = GameState.Pause;
            AudioManager.Instance?.PauseMusic();
        }
        else
        {
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
            State = GameState.Playing;
            AudioManager.Instance?.ResumeMusic();
        }
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

    public void ReturnToMenu()
    {
        levelNumber = 1;
        AudioManager.Instance?.StopMusic();
        SceneLoader.LoadScene(Scenes.HomeScene);
        State = GameState.Playing;
    }

    public void RequestControls() => OnControlsRequested?.Invoke(this, EventArgs.Empty);
}
