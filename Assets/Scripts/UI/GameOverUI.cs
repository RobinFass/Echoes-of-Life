using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private Action nextButtonClickAction;

    private Player player => Player.Instance;

    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            GameManager.Instance.NextLevel();
            Hide();
        });
        retryButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RestartLevel();
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
        player.Stats.OnDeath += GameManager_OnDeath;
        player.OnPlayerWin += Player_OnPlayerWin;
        Hide();
    }

    private void Player_OnPlayerWin(object sender, EventArgs e)
    {
        titleText.text = "<color=#00ff00>You Win!</color>";

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(false);

        Show();
    }

    private void GameManager_OnDeath(object sender, EventArgs e)
    {
        titleText.text = "<color=#ff0000>You Died!</color>";
        
        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);
        
        Show();
    }

    private void Show()
    {
        if (retryButton.gameObject.activeSelf)
            retryButton.Select();
        else
            nextButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}