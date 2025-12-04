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
            backButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayClick();
                Hide();
            });
        }

        private void Start()
        {
            Hide();
            gameManager.OnControlsRequested += GameManager_OnControlsRequested;
        }

        private void GameManager_OnControlsRequested(object sender, EventArgs e)
        {
            backButton.Select();
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}