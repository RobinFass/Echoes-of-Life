using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button continueButton;
    
    private Player player => Player.Instance;
    private bool firstTimeFinalDeath = false;

    private void Awake()
    {
        retryButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            GameManager.Instance.RestartLevel();
            Hide();
        });
        homeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            SceneLoader.LoadScene(Scenes.HomeScene);
            Hide();
        });
        continueButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
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
        AudioManager.Instance?.StopLoopingSfx();
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlaySfx("die");
        if (e.IsFinalBoss && !firstTimeFinalDeath)
        {
            firstTimeFinalDeath = true;
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