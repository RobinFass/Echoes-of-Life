using UnityEngine;
using UnityEngine.UI;

public class NextLevelUI : MonoBehaviour
{
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button continueButton;
    
    private void Awake()
    {
        nextButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene((Scenes)(++GameManager.levelNumber));
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
        nextButton.gameObject.SetActive(false);
        homeButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerWin += Player_OnPlayerWin;
        Hide();
    }

    private void Player_OnPlayerWin(object sender, Enemy e)
    {
        if (e.IsFinalBoss)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.Select();
            gameObject.SetActive(true);
            return;
        }
        
        nextButton.gameObject.SetActive(true);
        homeButton.gameObject.SetActive(true);
        
        nextButton.Select();
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}