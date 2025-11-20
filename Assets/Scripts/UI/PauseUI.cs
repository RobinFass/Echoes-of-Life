using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button controlsButton;

    private static GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        continueButton.onClick.AddListener(() =>
        {
            Hide();
            gameManager?.ResumeGame();
        });
        homeButton.onClick.AddListener(() =>
        {
            Hide();
            gameManager?.ReturnToMenu();
        });
        controlsButton.onClick.AddListener(() =>
        {
            gameManager?.RequestControls();
        });
    }

    private void Start()
    {
        gameManager.OnGamePaused += GameManager_OnGamePause;
        gameManager.OnGameUnpaused += GameManager_OnGameUnpause;
        Hide();
    }

    private void GameManager_OnGamePause(object sender, EventArgs e) => Show();

    private void GameManager_OnGameUnpause(object sender, EventArgs e) => Hide();

    private void Show()
    {
        continueButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
