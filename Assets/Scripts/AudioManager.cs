using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NamedClip { public string key; public AudioClip clip; }
[System.Serializable]
public class LevelMusicMapping { public int level; public string key; }

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
    private readonly List<AudioSource> sfxPool = new();
    private int sfxPoolIndex;

    [Header("Auto Music on Scene Load (unused with prefab levels)")]
    [SerializeField] private bool autoPlayOnSceneLoad = false;
    [SerializeField] private string level1SceneName = "Level1";
    [SerializeField] private string level1MusicKey = "ambiance1";
    [SerializeField] private bool playOnlyOncePerSession = false;

    private readonly Dictionary<string, AudioClip> musicMap = new();
    private readonly Dictionary<string, AudioClip> sfxMap = new();
    
    private AudioSource musicSource;
    private AudioSource loopingSfxSource;
    private bool hasPlayedLevel1Music;
    private string currentMusicKey;
    private bool isMusicPaused;
    private bool isLoopingSfxPaused;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        if (autoPlayOnSceneLoad)
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        foreach (var n in musicClips)
            if (!string.IsNullOrEmpty(n.key) && n.clip) musicMap[n.key] = n.clip;
        foreach (var n in sfxClips)
            if (!string.IsNullOrEmpty(n.key) && n.clip) sfxMap[n.key] = n.clip;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true; // do not be affected by global listener pause

        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            src.loop = false;
            sfxPool.Add(src);
        }
        // one AudioSource used for looping SFX (walk, run, etc.)
        loopingSfxSource = gameObject.AddComponent<AudioSource>();
        loopingSfxSource.outputAudioMixerGroup = sfxGroup;
        loopingSfxSource.loop = true;
        loopingSfxSource.playOnAwake = false;
    }
    
    public void PlayLoopingSfx(string key)
    {
        if (!sfxMap.TryGetValue(key, out var clip)) return;
        if (loopingSfxSource.isPlaying && loopingSfxSource.clip == clip) return;

        loopingSfxSource.clip = clip;
        loopingSfxSource.Play();
    }

    public void StopLoopingSfx()
    {
        loopingSfxSource.Stop();
        loopingSfxSource.clip = null;
    }

    public void PlaySfx(string key, float volume = 1f)
    {
        if (!sfxMap.TryGetValue(key, out var clip) || !clip) return;
        PlaySfx(clip, volume);
    }

    private void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
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
        if (!musicMap.TryGetValue(key, out var clip) || !clip)
        {
            Debug.LogWarning($"AudioManager: music key '{key}' not found.");
            return;
        }
        currentMusicKey = key;
        PlayMusic(clip, volume);
    }

    private void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
        bool restarting = musicSource.clip != clip;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(volume);
        if (!musicSource.isPlaying || restarting)
        {
            isMusicPaused = false;
            musicSource.Stop();
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        currentMusicKey = null;
        isMusicPaused = false;
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
            isMusicPaused = true;
        }
    }

    public void ResumeMusic()
    {
        if (musicSource == null || musicSource.clip == null)
        {
            Debug.Log("AudioManager: no music to resume.");
            return;
        }

        if (isMusicPaused)
        {
            musicSource.UnPause();
            isMusicPaused = false;
            return;
        }

        if (!musicSource.isPlaying)
            musicSource.Play();
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
        { 
            sfxGroup.audioMixer.SetFloat("SfxVolume",
                linear <= 0.0001f ? -80f : Mathf.Log10(Mathf.Clamp01(linear)) * 20f);
        }
    }

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
            var fallback = $"ambiance{level}"; // match your naming
            if (musicMap.ContainsKey(fallback)) key = fallback;
        }

        if (!string.IsNullOrEmpty(key))
        {
            PlayMusic(key, volume);
        }
        else
        {
            Debug.LogWarning($"AudioManager: no music mapping found for level {level}.");
        }
    }
    
    public void PlayBossMusic(int level, float volume = 1f)
    {
        var key = $"boss{Mathf.Clamp(level, 1, 3)}";
        Debug.Log($"AudioManager: playing boss music '{key}' for level {level}.");
        PlayMusic(key, volume);
    }
}
