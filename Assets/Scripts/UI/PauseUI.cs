using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;


    private static GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        continueButton.onClick.AddListener(() =>
        {
            gameManager.State = GameState.Playing;
            Hide();
        });
        homeButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(Scenes.HomeScene);
            Hide();
        });
    }
    private void Start()
    {
        gameManager.OnGamePaused += GameManager_OnGamePause;
        gameManager.OnGameUnpaused += GameManager_OnGameUnpause;
        Hide();
    }
    
    private void GameManager_OnGamePause(object sender, EventArgs e)
    {
        Show();
    }
    
    private void GameManager_OnGameUnpause(object sender, EventArgs e)
    {
        Hide();
    }
    
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