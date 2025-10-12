using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void FixedUpdate()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + _gameManager.GetScore();
        healthBar.fillAmount = _gameManager.GetHealthNormalized();
        staminaBar.fillAmount = Player.Instance.GetStaminaNormalized();

    }
}
