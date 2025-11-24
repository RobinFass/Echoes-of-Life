using System;
using UnityEngine;
using UnityEngine.UI;
// known bug but i thinks it's fine
// if the player hit pause then controls and escape, the player can play but the controls ui
// will still be there until the player hit back button
namespace UI
{
    public class ControlsUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;

        private GameManager gameManager => GameManager.Instance;

        private void Awake()
        {
            backButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            Hide();
            gameManager.OnControlsRequested += GameManager_OnControlsRequested;
            gameManager.OnGameUnpaused += GameManager_OnGameUnpause;
        }
        
        private void GameManager_OnGameUnpause(object sender, EventArgs e)
        {
            Hide();
        }

        private void GameManager_OnControlsRequested(object sender, EventArgs e)
        {
            Show();
        }
        
        private void Show()
        {
            backButton.Select();
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            if(GameInput.Instance) GameInput.Instance.UpdateKeys();
            gameObject.SetActive(false);
        }
    }
}