using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button controlsButton;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private static GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        AudioManager.Instance?.StopLoopingSfx();

        continueButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            Hide();
            gameManager?.ResumeGame();
        });

        homeButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlaySfx("click");
            Hide();
            gameManager?.ReturnToMenu();
        });

        controlsButton.onClick.AddListener(() =>
        {
            gameManager?.RequestControls();
        });

        // Music slider
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        musicSlider.wholeNumbers = false;

        // Load saved value and apply
        float musicVol = PlayerPrefs.GetFloat("volume_music", 1f);
        musicSlider.value = musicVol;
        AudioManager.Instance?.SetMusicVolume(musicVol);

        musicSlider.onValueChanged.AddListener(v =>
        {
            AudioManager.Instance?.SetMusicVolume(v);
            float currentSfx = sfxSlider.value;
            AudioManager.Instance?.SaveVolumes(v, currentSfx);
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
            float currentMusic = musicSlider.value;
            AudioManager.Instance?.SaveVolumes(currentMusic, v);
        });
    }

    private void Start()
    {
        gameManager.OnGamePaused += GameManager_OnGamePause;
        gameManager.OnGameUnpaused += GameManager_OnGameUnpause;
        gameManager.OnControlsRequested += GameManager_OnControlsRequested;
        Hide();
    }

    private void GameManager_OnGamePause(object sender, EventArgs e) => Show();

    private void GameManager_OnGameUnpause(object sender, EventArgs e) => Hide();

    private void GameManager_OnControlsRequested(object sender, EventArgs e) => Hide();

    private void Show()
    {
        continueButton.Select();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        // When opening pause, sync sliders to current saved values
        float musicVol = PlayerPrefs.GetFloat("volume_music", 1f);
        float sfxVol = PlayerPrefs.GetFloat("volume_sfx", 1f);
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
