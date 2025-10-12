using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private TextMeshProUGUI titletext;
    [SerializeField] private TextMeshProUGUI buttonText;

    private int currentButton = 0; // 0 = nextButton, 1 = homeButton
    private Action nextButtonClickAction;
    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            nextButtonClickAction();
            Hide();
        });
        homeButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerDeath += GameManager_OnPlayerDeath;
        Player.Instance.OnPlayerWin += Player_OnPlayerWin;
        Hide();
    }

    private void OnMove(InputValue value)
    {
        if (value.Get<Vector2>() == Vector2.left || value.Get<Vector2>() == Vector2.right)
        {
            currentButton = (currentButton + 1) % 2; // Toggle between 0 and 1;
            if (currentButton == 0)
            {
                nextButton.Select();
            }
            else
            {
                homeButton.Select();
            }
        }
    }
    
    private void OnSelect()
    {
        nextButton.onClick.Invoke();
    }

    private void Player_OnPlayerWin(object sender, EventArgs e)
    {
        titletext.text = "<color=#00ff00>You Win!</color>";
        buttonText.text = "Continue";
        nextButtonClickAction = () =>
        {
            GameManager.Instance.NextLevel();
        };
        
        Show();
    }

    private void GameManager_OnPlayerDeath(object sender, EventArgs e)
    {
        titletext.text = "<color=#ff0000>You Died!</color>";
        buttonText.text = "Retry";
        nextButtonClickAction = () =>
        {
            GameManager.Instance.RestartLevel();
        };
        Show();
    }

    private void Show()
    {
        nextButton.Select();
        currentButton = 0;
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
