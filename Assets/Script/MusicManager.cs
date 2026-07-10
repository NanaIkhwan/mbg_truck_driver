using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Setting")]
    public string gameSceneName = "Game"; // ← isi nama scene game kamu

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton — hanya ada 1 MusicManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ← tidak hilang saat pindah scene

        audioSource = GetComponent<AudioSource>();

        // Dengarkan event saat scene berganti
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            // Masuk scene Game → stop musik
            audioSource.Stop();
        }
        else
        {
            // Balik ke MainMenu → play musik lagi
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    // Dipanggil SettingManager untuk atur volume
    public void SetVolume(float value)
    {
        audioSource.volume = value;
    }

    public float GetVolume()
    {
        return audioSource.volume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}