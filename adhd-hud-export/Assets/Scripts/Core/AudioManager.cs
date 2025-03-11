using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Sound Effects")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();

    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private bool muteAll = false;
    [SerializeField] private bool enableSpatialAudio = true;

    private Dictionary<string, SoundEffect> soundEffectDict = new Dictionary<string, SoundEffect>();
    private AudioListener audioListener;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        InitializeAudioSources();
        LoadAudioSettings();
    }

    private void InitializeAudioSources()
    {
        foreach (var sound in soundEffects)
        {
            if (sound.clip != null)
            {
                // Create audio source
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.volume = sound.volume * masterVolume;
                source.pitch = sound.pitch;
                source.loop = sound.loop;
                source.spatialBlend = enableSpatialAudio ? 1f : 0f;

                sound.source = source;
                soundEffectDict[sound.name] = sound;
            }
        }
    }

    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        muteAll = PlayerPrefs.GetInt("MuteAll", 0) == 1;
        enableSpatialAudio = PlayerPrefs.GetInt("SpatialAudio", 1) == 1;

        UpdateAudioSettings();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetInt("MuteAll", muteAll ? 1 : 0);
        PlayerPrefs.SetInt("SpatialAudio", enableSpatialAudio ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlaySound(string soundName, Vector3? position = null)
    {
        if (muteAll) return;

        if (soundEffectDict.TryGetValue(soundName, out SoundEffect sound))
        {
            if (position.HasValue && enableSpatialAudio)
            {
                sound.source.transform.position = position.Value;
            }

            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found!");
        }
    }

    public void StopSound(string soundName)
    {
        if (soundEffectDict.TryGetValue(soundName, out SoundEffect sound))
        {
            sound.source.Stop();
        }
    }

    public void PauseSound(string soundName)
    {
        if (soundEffectDict.TryGetValue(soundName, out SoundEffect sound))
        {
            sound.source.Pause();
        }
    }

    public void ResumeSound(string soundName)
    {
        if (soundEffectDict.TryGetValue(soundName, out SoundEffect sound))
        {
            sound.source.UnPause();
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSettings();
        SaveAudioSettings();
    }

    public void ToggleMute()
    {
        muteAll = !muteAll;
        UpdateAudioSettings();
        SaveAudioSettings();
    }

    public void ToggleSpatialAudio()
    {
        enableSpatialAudio = !enableSpatialAudio;
        UpdateAudioSettings();
        SaveAudioSettings();
    }

    private void UpdateAudioSettings()
    {
        foreach (var sound in soundEffects)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * masterVolume * (muteAll ? 0f : 1f);
                sound.source.spatialBlend = enableSpatialAudio ? 1f : 0f;
            }
        }
    }

    // Notification sounds
    public void PlayTaskCompleteSound()
    {
        PlaySound("task_complete");
    }

    public void PlayPomodoroCompleteSound()
    {
        PlaySound("pomodoro_complete");
    }

    public void PlayBreakReminderSound()
    {
        PlaySound("break_reminder");
    }

    public void PlayWaterReminderSound()
    {
        PlaySound("water_reminder");
    }

    // Debug methods
    public void TestAllSounds()
    {
        foreach (var sound in soundEffects)
        {
            PlaySound(sound.name);
        }
    }

    public void TestSpatialAudio()
    {
        if (Camera.main != null)
        {
            Vector3 leftPosition = Camera.main.transform.position + Vector3.left * 2f;
            Vector3 rightPosition = Camera.main.transform.position + Vector3.right * 2f;

            PlaySound("task_complete", leftPosition);
            PlaySound("pomodoro_complete", rightPosition);
        }
    }
} 