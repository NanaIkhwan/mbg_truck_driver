using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MisiSelesaiManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject misiSelesaiPanel;

    [Header("Teks Hasil")]
    public TMP_Text titleText;
    public TMP_Text statDiantarText;
    public TMP_Text statWaktuText;
    public TMP_Text statSkorText;

    [Header("Tombol")]
    public Button replayButton;
    public Button mainMenuButton;

    [Header("Setting Scene")]
    public string mainMenuSceneName = "MainMenu"; // nama scene menu utama

    private void Start()
    {
        // Sembunyikan panel saat awal
        if (misiSelesaiPanel != null)
            misiSelesaiPanel.SetActive(false);

        // Hubungkan tombol
        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplay);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    // Dipanggil oleh DeliveryManager saat semua misi selesai
    public void ShowMisiSelesai(int totalDiantar, float waktuDetik, int skor)
    {
        misiSelesaiPanel.SetActive(true);

        // Hitung format waktu
        int minutes = Mathf.FloorToInt(waktuDetik / 60f);
        int seconds = Mathf.FloorToInt(waktuDetik % 60f);

        titleText.text      = "Misi Selesai!";
        statDiantarText.text = totalDiantar + "/" + totalDiantar;
        statWaktuText.text   = string.Format("{0:00}:{1:00}", minutes, seconds);
        statSkorText.text    = skor.ToString();

        // Pause game
        Time.timeScale = 0f;
    }

    void OnReplay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}