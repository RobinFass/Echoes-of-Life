using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextLevelUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;
    
    private Player player => Player.Instance;

    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            GameManager.Instance.NextLevel();
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
        player.OnPlayerWin += Player_OnPlayerWin;
        Hide();
    }

    private void Player_OnPlayerWin(object sender, EventArgs e)
    {
        nextButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}