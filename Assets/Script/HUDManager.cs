using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public TMP_Text timerText;
    public TMP_Text speedText;

    [Header("Referensi")]
    public mobil truckMobil;
    public Rigidbody truckRb;

    private float elapsedTime = 0f;
    private bool hudActive = false;

    public void StartHUD()
    {
        hudActive = true;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (!hudActive) return;

        // Timer
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Speedometer dari Rigidbody
        if (speedText != null && truckRb != null)
        {
            float speed = truckRb.linearVelocity.magnitude * 3.6f;
            speedText.text = Mathf.RoundToInt(speed) + " km/h";
        }
    }

    public void StopHUD()
    {
        hudActive = false;
    }
}