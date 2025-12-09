using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ControlsUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;

        private GameManager gameManager => GameManager.Instance;
        private bool isOpen; // track if controls UI is currently shown

        private void Awake()
        {
            backButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayClick();
                // Back behaves like Escape: go back to Pause
                Hide();
                gameManager.CloseControlsToPause();
            });
        }

        private void Start()
        {
            Hide();
            gameManager.OnControlsRequested += GameManager_OnControlsRequested;
            gameManager.OnGameUnpaused += GameManager_OnGameUnpause;
            gameManager.OnGamePaused += GameManager_OnGamePause;
        }

        private void OnDestroy()
        {
            if (gameManager == null) return;
            gameManager.OnControlsRequested -= GameManager_OnControlsRequested;
            gameManager.OnGameUnpaused -= GameManager_OnGameUnpause;
            gameManager.OnGamePaused -= GameManager_OnGamePause;
        }
        
        private void GameManager_OnGameUnpause(object sender, EventArgs e)
        {
            // If Escape is pressed while Controls are open, return to Pause instead of resuming
            if (isOpen)
            {
                Hide();
                gameManager.CloseControlsToPause(); // PauseUI will show
            }
        }

        private void GameManager_OnGamePause(object sender, EventArgs e)
        {
            // Safety: if Controls were open and a pause event is fired, ensure they don't stay visible simultaneously.
            if (isOpen) Hide();
        }

        private void GameManager_OnControlsRequested(object sender, EventArgs e)
        {
            Show();
        }
        
        private void Show()
        {
            isOpen = true;
            backButton.Select();
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            isOpen = false;
            if(GameInput.Instance) GameInput.Instance.UpdateKeys();
            gameObject.SetActive(false);
        }
    }
}