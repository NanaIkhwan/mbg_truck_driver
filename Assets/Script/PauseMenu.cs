using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu Panel")]
    [Tooltip("Drag Panel pause menu ke sini")]
    public GameObject pausePanel;

    [Header("Tombol BACK di HUD")]
    [Tooltip("Drag Button BACK ke sini (bukan Panel, tapi Button-nya)")]
    public GameObject backButton;

    [Header("Panel Title")]
    [Tooltip("Drag TMP Text judul panel ke sini")]
    public TMP_Text panelName;

    private bool isPaused = false;

    private const string MAIN_MENU_SCENE = "MainMenu";

    // =====================
    // SET PANEL NAME
    // =====================
    public void SetPanelName(string name)
    {
        if (panelName == null)
        {
            Debug.LogWarning("panelName belum di-assign di Inspector!");
            return;
        }
        panelName.text = name;
    }

    private void Start()
    {
        // Panel pause disembunyikan saat mulai
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Tombol BACK tetap tampil
        if (backButton != null)
            backButton.SetActive(true);
    }

    private void Update()
    {
        // Escape / Android back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // =====================
    // PAUSE — dipanggil oleh Button BACK
    // =====================
    public void Pause()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);  // Tampilkan panel pause

        // Tombol BACK tetap tampil, TIDAK disembunyikan
        if (backButton != null)
            backButton.SetActive(true);

        Time.timeScale = 0f;  // <<< Freeze game
        isPaused = true;

        SetPanelName("PAUSED");
        Debug.Log("Game Paused - timeScale: " + Time.timeScale);
    }

    // =====================
    // RESUME — dipanggil oleh ButtonResume
    // =====================
    public void Resume()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);  // Sembunyikan panel pause

        // Tombol BACK tetap tampil
        if (backButton != null)
            backButton.SetActive(true);

        Time.timeScale = 1f;  // <<< Unfreeze game
        isPaused = false;

        SetPanelName("PLAYING");
        Debug.Log("Game Resumed - timeScale: " + Time.timeScale);
    }

    // =====================
    // EXIT TO MAIN MENU — dipanggil oleh ButtonExit
    // =====================
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;  // Reset sebelum pindah scene!
        Debug.Log("Exiting to Main Menu...");
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}