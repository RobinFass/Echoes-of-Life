using System;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ControlsUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;

        private static GameManager GameManager => GameManager.Instance;
        private bool isOpen;

        private void Awake()
        {
            backButton.onClick.AddListener(() =>
            {
                Hide();
                GameManager.CloseControlsToPause();
            });
        }

        private void Start()
        {
            Hide();
            GameManager.OnControlsRequested += GameManager_OnControlsRequested;
            GameManager.OnGameUnpaused += GameManager_OnGameUnpause;
            GameManager.OnGamePaused += GameManager_OnGamePause;
        }

        private void OnDestroy()
        {
            if (GameManager == null) return;
            GameManager.OnControlsRequested -= GameManager_OnControlsRequested;
            GameManager.OnGameUnpaused -= GameManager_OnGameUnpause;
            GameManager.OnGamePaused -= GameManager_OnGamePause;
        }
        
        private void GameManager_OnGameUnpause(object sender, EventArgs e)
        {
            if (!isOpen) return;
            Hide();
            GameManager.CloseControlsToPause(); // PauseUI will show
        }

        private void GameManager_OnGamePause(object sender, EventArgs e)
        {
            if (isOpen) Hide();
        }

        private void GameManager_OnControlsRequested(object sender, EventArgs e)
        {
            Show();
        }
        
        private void Show()
        {
            isOpen = true;
            AudioManager.Instance?.PlaySfx("open");
            backButton.Select();
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (isOpen) AudioManager.Instance?.PlaySfx("close");
            isOpen = false;
            if(GameInput.Instance) GameInput.Instance.UpdateKeys();
            gameObject.SetActive(false);
        }
    }
}
