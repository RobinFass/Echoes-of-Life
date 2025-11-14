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
        playButton.onClick.AddListener(() => { SceneLoader.LoadScene(Scenes.GameScene); });
        quitButton.onClick.AddListener(Application.Quit);
        controlsButton.onClick.AddListener(() => { gameManager.RequestControls(); });
    }

    private void Start()
    {
        playButton.Select();
    }
}