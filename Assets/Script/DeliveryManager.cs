using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeliveryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject missionPanel;
    public TMP_Text missionText;
    public TMP_Text deliveryCountText;
    public RectTransform arrowIndicator;

    [Header("Delivery Points")]
    public Transform[] deliveryPoints;
    private int currentDelivery = 0;

    [Header("Referensi")]
    public Transform truck;

    [Header("HUD")]
    public HUDManager hudManager;

    [Header("Misi Selesai")]
    public MisiSelesaiManager misiSelesaiManager;

    private bool missionActive = false;
    private float totalWaktu = 0f;
    private int skor = 0;

    void Update()
    {
        if (missionActive)
            totalWaktu += Time.deltaTime;

        if (!missionActive) return;
        if (currentDelivery >= deliveryPoints.Length) return;
        if (arrowIndicator == null) return;

        Vector3 targetPos = deliveryPoints[currentDelivery].position;
        Vector3 truckPos  = truck.position;
        Vector3 direction = targetPos - truckPos;
        direction.y = 0;

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle -= truck.eulerAngles.y;
        arrowIndicator.rotation = Quaternion.Euler(0, 0, -angle);
    }

    public void StartMission()
    {
        missionActive = true;
        totalWaktu = 0f;
        skor = 0;
        missionPanel.SetActive(true);
        arrowIndicator.gameObject.SetActive(true);
        if (hudManager != null) hudManager.StartHUD();
        UpdateMissionUI();
    }

    void UpdateMissionUI()
    {
        if (currentDelivery < deliveryPoints.Length)
        {
            missionText.text = "Antar makanan ke:\n" + deliveryPoints[currentDelivery].name;
            deliveryCountText.text = "Pengiriman: " + currentDelivery + "/" + deliveryPoints.Length;
        }
    }

    public void OnDeliveryComplete(string zoneName)
    {
        currentDelivery++;
        skor += 100;
        deliveryCountText.text = "Pengiriman: " + currentDelivery + "/" + deliveryPoints.Length;

        if (currentDelivery >= deliveryPoints.Length)
        {
            missionActive = false;
            arrowIndicator.gameObject.SetActive(false);
            if (hudManager != null) hudManager.StopHUD();
            if (misiSelesaiManager != null)
                misiSelesaiManager.ShowMisiSelesai(currentDelivery, totalWaktu, skor);
        }
        else
        {
            missionText.text = "Makanan diantar!\nTujuan berikutnya:\n"
                             + deliveryPoints[currentDelivery].name;
            UpdateMissionUI();
        }
    }

    public bool IsLastDelivery()
    {
        return currentDelivery >= deliveryPoints.Length - 1;
    }
}