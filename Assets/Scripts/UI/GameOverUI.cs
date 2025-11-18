using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;
    
    private Player player => Player.Instance;

    private void Awake()
    {
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
        continueButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(Scenes.EndScene);
            Hide();
        });
        homeButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        player.Stats.OnDeath += GameManager_OnDeath;
        Hide();
    }
    

    private void GameManager_OnDeath(object sender, Enemy e)
    {
        if (e.IsFinalBoss)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.Select();
            gameObject.SetActive(true);
            return;
        }
        
        retryButton.gameObject.SetActive(true);
        homeButton.gameObject.SetActive(true);
        
        retryButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}