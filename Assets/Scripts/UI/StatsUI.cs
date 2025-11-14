using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;

    private GameManager gameManager => GameManager.Instance;
    private PlayerStats stats => Player.Instance.Stats;


    private void FixedUpdate()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        healthBar.fillAmount = stats.NormalizedHealth;
        staminaBar.fillAmount = stats.NormalizedStamina;
    }
}