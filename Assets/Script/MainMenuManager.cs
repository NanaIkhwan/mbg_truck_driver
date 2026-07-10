using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    private const string GAME_SCENE = "level_1";

    [Header("Panel Title")]
    public TMP_Text panelName;

    [Header("Tombol Continue (opsional)")]
    public Button buttonContinue; // drag tombol Continue di Inspector

    public void SetPanelName(string name)
    {
        if (panelName != null)
            panelName.text = name;
    }

    private void Start()
    {
        SetPanelName("MAIN MENU");

        // Tombol Continue hanya aktif kalau ada save
        if (buttonContinue != null)
            buttonContinue.interactable = SaveManager.HasSave();
    }

    public void PlayGame()
    {
        SetPanelName("NEW GAME");
        SaveManager.DeleteSave();

        // Tandai New Game
        PlayerPrefs.SetInt("IsContinue", 0);
        PlayerPrefs.Save();

        Debug.Log("Starting new game...");
        SceneManager.LoadScene(GAME_SCENE);
    }

    public void ContinueGame()
    {
        SetPanelName("CONTINUE");

        if (SaveManager.HasSave())
        {
            // Tandai Continue
            PlayerPrefs.SetInt("IsContinue", 1);
            PlayerPrefs.Save();

            Debug.Log("Continuing game...");
            SceneManager.LoadScene(GAME_SCENE);
        }
        else
        {
            Debug.LogWarning("No save data found!");
            // Tombol harusnya sudah disabled, tapi jaga-jaga
            buttonContinue.interactable = false;
        }
    }

    public void ExitGame()
    {
        SetPanelName("EXIT GAME");
        Debug.Log("Exiting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool HasSaveData()
    {
        return SaveManager.HasSave();
    }
}