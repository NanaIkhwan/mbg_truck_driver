using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public Slider sliderMusic;
    public Slider sliderSFX;

    [Header("Toggle Mute")]
    public Toggle toggleMute;

    [Header("Toggle Shadow")]
    public Toggle toggleShadow;

    [Header("Kualitas Grafis")]
    public TMPro.TMP_Dropdown dropdownQuality;

    void Start()
    {
        // Load saved settings
        sliderMusic.value   = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sliderSFX.value     = PlayerPrefs.GetFloat("SFXVolume", 0.85f);
        toggleMute.isOn     = PlayerPrefs.GetInt("Mute", 0) == 1;
        toggleShadow.isOn   = PlayerPrefs.GetInt("Shadow", 1) == 1;
        dropdownQuality.value = PlayerPrefs.GetInt("Quality", 1);

        // Apply semua setting saat start
        ApplyMusicVolume(sliderMusic.value);
        ApplySFXVolume(sliderSFX.value);
        ApplyMute(toggleMute.isOn);
        ApplyShadow(toggleShadow.isOn);
        ApplyQuality(dropdownQuality.value);

        // Hubungkan listener
        sliderMusic.onValueChanged.AddListener(ApplyMusicVolume);
        sliderSFX.onValueChanged.AddListener(ApplySFXVolume);
        toggleMute.onValueChanged.AddListener(ApplyMute);
        toggleShadow.onValueChanged.AddListener(ApplyShadow);
        dropdownQuality.onValueChanged.AddListener(ApplyQuality);
    }

    void ApplyMusicVolume(float value)
    {
        if (musicSource != null) musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void ApplySFXVolume(float value)
    {
        if (sfxSource != null) sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void ApplyMute(bool isMuted)
    {
        AudioListener.volume = isMuted ? 0f : 1f;
        PlayerPrefs.SetInt("Mute", isMuted ? 1 : 0);
    }

    void ApplyShadow(bool isOn)
    {
        QualitySettings.shadows = isOn ? ShadowQuality.All : ShadowQuality.Disable;
        PlayerPrefs.SetInt("Shadow", isOn ? 1 : 0);
    }

    void ApplyQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}