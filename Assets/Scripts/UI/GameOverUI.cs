using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI nextButtonText;
    
    private Action nextButtonClickAction;
    
    private Player player => Player.Instance;
    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction();
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
        nextButtonText.text = "Continue";
        nextButtonClickAction = () =>
        {
            GameManager.Instance.NextLevel();
        };
        
        Show();
    }

    private void GameManager_OnDeath(object sender, EventArgs e)
    {
        titleText.text = "<color=#ff0000>You Died!</color>";
        nextButtonText.text = "Retry";
        nextButtonClickAction = () =>
        {
            GameManager.Instance.RestartLevel();
        };
        Show();
    }

    private void Show()
    {
        nextButton.Select();
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
