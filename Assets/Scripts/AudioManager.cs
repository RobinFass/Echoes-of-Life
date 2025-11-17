// csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NamedClip
{
    public string key;
    public AudioClip clip;
}

[System.Serializable]
public class LevelMusicMapping
{
    public int level;
    public string key; // must match a key from musicClips
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Clips (assign in inspector)")]
    [SerializeField] private List<NamedClip> musicClips = new();
    [SerializeField] private List<NamedClip> sfxClips = new();

    [Header("Level Music Mapping")]
    [SerializeField] private List<LevelMusicMapping> levelMusic = new();

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 8;
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Auto Music on Scene Load (unused with prefab levels)")]
    [SerializeField] private bool autoPlayOnSceneLoad = false;
    [SerializeField] private string level1SceneName = "Level1";
    [SerializeField] private string level1MusicKey = "ambience1";
    [SerializeField] private bool playOnlyOncePerSession = false;

    private Dictionary<string, AudioClip> musicMap = new();
    private Dictionary<string, AudioClip> sfxMap = new();

    private AudioSource musicSource;
    private List<AudioSource> sfxPool = new();
    private int sfxPoolIndex = 0;

    private bool hasPlayedLevel1Music;
    private string currentMusicKey;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (autoPlayOnSceneLoad)
        {
            var current = SceneManager.GetActiveScene();
            OnSceneLoaded(current, LoadSceneMode.Single);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!autoPlayOnSceneLoad) return;

        if (!string.IsNullOrEmpty(level1SceneName) &&
            scene.name.Equals(level1SceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            if (!playOnlyOncePerSession || !hasPlayedLevel1Music)
            {
                PlayMusic(level1MusicKey, 1f);
                hasPlayedLevel1Music = true;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        foreach (var n in musicClips)
            if (n != null && !string.IsNullOrEmpty(n.key) && n.clip != null)
                musicMap[n.key] = n.clip;

        foreach (var n in sfxClips)
            if (n != null && !string.IsNullOrEmpty(n.key) && n.clip != null)
                sfxMap[n.key] = n.clip;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            src.loop = false;
            sfxPool.Add(src);
        }
    }

    public void PlaySfx(string key, float volume = 1f)
    {
        if (!sfxMap.TryGetValue(key, out var clip) || clip == null) return;
        PlaySfx(clip, volume);
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        var src = sfxPool[sfxPoolIndex];
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Count;
        src.volume = Mathf.Clamp01(volume);
        src.clip = clip;
        src.PlayOneShot(clip, src.volume);
    }

    public void PlayMusic(string key, float volume = 1f)
    {
        if (currentMusicKey == key && musicSource.isPlaying)
        {
            musicSource.volume = Mathf.Clamp01(volume);
            return;
        }

        if (!musicMap.TryGetValue(key, out var clip) || clip == null)
        {
            Debug.LogWarning($"AudioManager: music key '{key}' not found.");
            return;
        }

        currentMusicKey = key;
        PlayMusic(clip, volume);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        if (!musicSource.isPlaying) musicSource.Play();
        else
        {
            // restart with new clip if different
            musicSource.Stop();
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        currentMusicKey = null;
        musicSource.Stop();
    }

    public void SetMusicVolume(float linear)
    {
        if (musicGroup?.audioMixer != null)
            musicGroup.audioMixer.SetFloat("MusicVolume",
                linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f);
    }

    public void SetSfxVolume(float linear)
    {
        if (sfxGroup?.audioMixer != null)
            sfxGroup.audioMixer.SetFloat("SfxVolume",
                linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f);
    }

    // New: play music based on level number (called by GameManager)
    public void PlayLevelMusic(int level, float volume = 1f)
    {
        string key = null;
        foreach (var m in levelMusic)
        {
            if (m != null && m.level == level && !string.IsNullOrEmpty(m.key))
            {
                key = m.key;
                break;
            }
        }

        if (string.IsNullOrEmpty(key))
        {
            // fallback: try "ambience{level}"
            var fallback = $"ambience{level}";
            if (musicMap.ContainsKey(fallback)) key = fallback;
        }

        if (!string.IsNullOrEmpty(key))
            PlayMusic(key, volume);
        else
            Debug.LogWarning($"AudioManager: no music mapping found for level {level}.");
    }
}
