using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // Nama scene game kamu (sesuai gambar ke-2)
    private const string GAME_SCENE = "level_1";

    // Key untuk menyimpan data save (PlayerPrefs)
    private const string SAVE_KEY = "HasSaveData";

    [Header("Panel Title")]
    [Tooltip("Drag TMP Text judul panel ke sini")]
    public TMP_Text panelName;

    // =====================
    // SET PANEL NAME
    // Bisa dipanggil dari mana saja untuk ganti judul panel
    // =====================
    public void SetPanelName(string name)
    {
        panelName.text = name;
    }

    private void Start()
    {
        // Judul awal saat scene pertama load
        SetPanelName("MAIN MENU");
    }

    // =====================
    // PLAY GAME
    // Mulai game baru dari awal, hapus save lama
    // =====================
    public void PlayGame()
    {
        SetPanelName("NEW GAME");

        // Hapus data save lama jika ada
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        Debug.Log("Starting new game...");
        SceneManager.LoadScene(GAME_SCENE);
    }

    // =====================
    // CONTINUE GAME
    // Lanjut game dari save terakhir
    // =====================
    public void ContinueGame()
    {
        SetPanelName("CONTINUE");

        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("Continuing game...");
            SceneManager.LoadScene(GAME_SCENE);
        }
        else
        {
            Debug.LogWarning("No save data found! Starting new game instead.");
            SceneManager.LoadScene(GAME_SCENE);
        }
    }

    // =====================
    // EXIT GAME
    // Keluar dari aplikasi
    // =====================
    public void ExitGame()
    {
        SetPanelName("EXIT GAME");
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        // Di Editor Unity, stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Di build asli, keluar aplikasi
        Application.Quit();
#endif
    }

    // =====================
    // CEK APAKAH ADA SAVE (opsional)
    // =====================
    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }
}