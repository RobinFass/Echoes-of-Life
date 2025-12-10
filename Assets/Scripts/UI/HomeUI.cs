using UnityEngine;
using UnityEngine.UI;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button controlsButton;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        // Buttons
        playButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            if (GameManager.levelNumber == 0)
            {
                GameManager.levelNumber = 1;
                SceneLoader.LoadScene(Scenes.PreLvl1Scene);
            }
            else
                SceneLoader.LoadScene(Scenes.GameScene);
        });

        quitButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            Application.Quit();
        });

        controlsButton.onClick.AddListener(() =>
        {
            gameManager.RequestControls();
        });

        // Music slider
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;

        // Load saved value first
        float musicVol = PlayerPrefs.GetFloat("volume_music", 1f);
        musicSlider.value = musicVol;
        AudioManager.Instance?.SetMusicVolume(musicVol);

        musicSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMusicVolume(v);
            // Save both volumes (using current SFX slider value if present)
            float sfxVol = sfxSlider != null ? sfxSlider.value : 1f;
            AudioManager.Instance?.SaveVolumes(v, sfxVol);
        });

        // SFX slider
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;
        sfxSlider.wholeNumbers = false;

        float sfxVol = PlayerPrefs.GetFloat("volume_sfx", 1f);
        sfxSlider.value = sfxVol;
        AudioManager.Instance?.SetSfxVolume(sfxVol);

        sfxSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetSfxVolume(v);
            float musicVol = musicSlider != null ? musicSlider.value : 1f;
            AudioManager.Instance?.SaveVolumes(musicVol, v);
        });
    }

    private void Start()
    {
        playButton.Select();
    }
}
