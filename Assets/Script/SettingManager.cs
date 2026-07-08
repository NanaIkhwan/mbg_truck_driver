using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;          // drag MusicManager
    public AudioSource[] sfxSources;         // drag Truck, DropZone, dll

    [Header("UI")]
    public Slider sliderMusic;
    public Slider sliderSFX;
    public Toggle toggleMute;

    // Static supaya bisa diakses TruckEngine, DropZone, dll
    public static float SFXVolume = 1f;
    public static bool IsMuted = false;

    void Start()
    {
        // Load saved settings
        sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sliderSFX.value   = PlayerPrefs.GetFloat("SFXVolume", 0.85f);
        toggleMute.isOn   = PlayerPrefs.GetInt("Mute", 0) == 1;

        // Apply saat start
        ApplyMusicVolume(sliderMusic.value);
        ApplySFXVolume(sliderSFX.value);
        ApplyMute(toggleMute.isOn);

        // Hubungkan listener
        sliderMusic.onValueChanged.AddListener(ApplyMusicVolume);
        sliderSFX.onValueChanged.AddListener(ApplySFXVolume);
        toggleMute.onValueChanged.AddListener(ApplyMute);
    }

    void ApplyMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void ApplySFXVolume(float value)
    {
        SFXVolume = value;

        // Apply ke semua AudioSource SFX yang di-assign
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