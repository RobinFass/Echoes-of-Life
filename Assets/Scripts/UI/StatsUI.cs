using Common.Player;
using Object;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StatsUI : MonoBehaviour
    {
        [SerializeField] private Image healthBar;
        [SerializeField] private Image staminaBar;
        
        private static PlayerStats Stats => Player.Stats;

        private void FixedUpdate()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            healthBar.fillAmount = Stats.NormalizedHealth;
            staminaBar.fillAmount = Stats.NormalizedStamina;
        }
    }
}
