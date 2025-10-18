using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Enemy enemy;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if(enemy.NormalizedHealth < 1f) 
            healthBar.fillAmount = enemy.NormalizedHealth;
    }
}
