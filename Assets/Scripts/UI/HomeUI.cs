using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button controlsButton;

    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        playButton.onClick.AddListener(() => {
            AudioManager.Instance?.PlayClick();
            SceneLoader.LoadScene(Scenes.GameScene);
        });
        quitButton.onClick.AddListener(() => {
            AudioManager.Instance?.PlayClick();
            Application.Quit();
        });
        controlsButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            gameManager.RequestControls();
        });
    }

    private void Start()
    {
        playButton.Select();
    }
}