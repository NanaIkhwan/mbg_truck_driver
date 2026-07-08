using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject tutorialPanel;
    public TMP_Text instructionText;
    public TMP_Text nextButtonText;
    public Button nextButton;

    [Header("Highlight Tombol (opsional)")]
    public GameObject highlightEngine;
    public GameObject highlightGas;
    public GameObject highlightBrake;
    public GameObject highlightSteer;

    [Header("Referensi")]
    public TruckEngine truckEngine;
    public mobil truckMobil;
    public SteeringWheel steeringWheel;
    public DeliveryManager deliveryManager;
    public GameObject missionPanel;

    private int currentStep = 0;

    private string[] instructions = {
        "Selamat datang, Driver MBG!\nSiap mengantar makanan ke sekolah?",
        "Nyalakan mesin dulu!\nTekan tombol Nyalakan Mesin.",
        "Tekan pedal Gas untuk maju.\nCoba sekarang!",
        "Tekan pedal Mundur.\nCoba sekarang!",
        "Geser stir untuk belok.\nCoba sekarang!",
        "Tekan pedal Rem.\nCoba sekarang!",
        "Hebat! Kamu siap berangkat.\nTekan Mulai Misi!"
    };

    void Start()
    {
        if (missionPanel != null) missionPanel.SetActive(false);
        ShowStep(0);
    }

    void ShowStep(int step)
    {
        SetHighlight(false, false, false, false);
        tutorialPanel.SetActive(true);
        instructionText.text = instructions[step];

        bool isFirst = step == 0;
        bool isLast  = step == instructions.Length - 1;
        nextButton.gameObject.SetActive(isFirst || isLast);

        if (nextButtonText != null)
            nextButtonText.text = isLast ? "Mulai Misi!" : "Lanjut";

        switch (step)
        {
            case 1:
                SetHighlight(true, false, false, false);
                StartCoroutine(WaitForEngine());
                break;
            case 2:
                SetHighlight(false, true, false, false);
                StartCoroutine(WaitForAction(() => truckMobil.IsGasPressed()));
                break;
            case 3:
                SetHighlight(false, true, false, false);
                StartCoroutine(WaitForAction(() => truckMobil.IsReversePressed()));
                break;
            case 4:
                SetHighlight(false, false, false, true);
                StartCoroutine(WaitForAction(() =>
                    Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
                    Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                    steeringWheel.isSteeringLeft || steeringWheel.isSteeringRight
                ));
                break;
            case 5:
                SetHighlight(false, false, true, false);
                StartCoroutine(WaitForAction(() => truckMobil.IsBrakePressed()));
                break;
        }
    }

    IEnumerator WaitForEngine()
    {
        while (truckEngine.GetEngineState() == TruckEngine.EngineState.Off)
            yield return null;

        NextStep();
    }

    IEnumerator WaitForAction(System.Func<bool> condition)
    {
        while (!condition())
            yield return null;

        yield return new WaitForSeconds(0.8f);
        NextStep();
    }

    void SetHighlight(bool engine, bool gas, bool brake, bool steer)
    {
        if (highlightEngine) highlightEngine.SetActive(engine);
        if (highlightGas)    highlightGas.SetActive(gas);
        if (highlightBrake)  highlightBrake.SetActive(brake);
        if (highlightSteer)  highlightSteer.SetActive(steer);
    }

    public void NextStep()
    {
        currentStep++;
        if (currentStep >= instructions.Length)
        {
            EndTutorial();
            return;
        }
        ShowStep(currentStep);
    }

    void EndTutorial()
    {
        tutorialPanel.SetActive(false);
        deliveryManager.StartMission();
    }
}