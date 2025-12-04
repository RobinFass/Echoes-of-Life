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
        playButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            if (GameManager.levelNumber == 0)
            {
                GameManager.levelNumber = 1;
                SceneLoader.LoadScene(Scenes.PreLvl1Scene);
            }
            else
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