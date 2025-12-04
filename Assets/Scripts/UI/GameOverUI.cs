using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton;
    
    private Player player => Player.Instance;

    private void Awake()
    {
        retryButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            GameManager.Instance.RestartLevel();
            Hide();
        });
        homeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            GameManager.Instance?.ReturnToMenu();
            Hide();
        });
    }

    private void Start()
    {
        player.Stats.OnDeath += GameManager_OnDeath;
        Hide();
    }
    

    private void GameManager_OnDeath(object sender, EventArgs e)
    {
        retryButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}