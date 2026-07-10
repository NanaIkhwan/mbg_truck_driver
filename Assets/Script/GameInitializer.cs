using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Referensi")]
    public DeliveryManager deliveryManager;
    public TutorialManager tutorialManager; // ← ganti jadi TutorialManager langsung

    void Start()
    {
        bool isContinue = PlayerPrefs.GetInt("IsContinue", 0) == 1;

        if (isContinue && SaveManager.HasSave())
        {
            // Nonaktifkan TutorialManager
            if (tutorialManager != null)
                tutorialManager.gameObject.SetActive(false);

            // Langsung continue
            if (deliveryManager != null)
                deliveryManager.ContinueMission();

            Debug.Log("[GameInitializer] Mode: Continue");
        }
        else
        {
            // New Game — panggil tutorial manual
            if (tutorialManager != null)
            {
                tutorialManager.gameObject.SetActive(true);
                tutorialManager.StartTutorial(); // ← panggil manual
            }

            Debug.Log("[GameInitializer] Mode: New Game");
        }
    }
}