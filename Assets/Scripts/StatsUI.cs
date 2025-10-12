using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;

    private GameManager gameManager => GameManager.Instance;
    private Player player => Player.Instance;


    private void FixedUpdate()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + gameManager.Score;
        healthBar.fillAmount = gameManager.NormalizedHealth;
        staminaBar.fillAmount = player.GetStaminaNormalized();

    }
}
