using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    private void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(Scenes.GameScene);
        });
        quitButton.onClick.AddListener(Application.Quit);
    }

    private void Start()
    {
        playButton.Select();
    }

}
