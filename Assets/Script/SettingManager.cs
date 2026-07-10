using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("UI")]
    public Slider sliderMusic;
    public Slider sliderSFX;
    public Toggle toggleMute;

    [Header("SFX Sources (khusus scene Game)")]
    public AudioSource[] sfxSources;

    public static float SFXVolume = 1f;
    public static bool IsMuted = false;

    void Start()
    {
        sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sliderSFX.value   = PlayerPrefs.GetFloat("SFXVolume", 0.85f);
        toggleMute.isOn   = PlayerPrefs.GetInt("Mute", 0) == 1;

        ApplyMusicVolume(sliderMusic.value);
        ApplySFXVolume(sliderSFX.value);
        ApplyMute(toggleMute.isOn);

        sliderMusic.onValueChanged.AddListener(ApplyMusicVolume);
        sliderSFX.onValueChanged.AddListener(ApplySFXVolume);
        toggleMute.onValueChanged.AddListener(ApplyMute);
    }

    void ApplyMusicVolume(float value)
    {
        // Akses MusicManager langsung via Instance
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void ApplySFXVolume(float value)
    {
        SFXVolume = value;
        foreach (var src in sfxSources)
            if (src != null) src.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void ApplyMute(bool isMuted)
    {
        IsMuted = isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        PlayerPrefs.SetInt("Mute", isMuted ? 1 : 0);
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}