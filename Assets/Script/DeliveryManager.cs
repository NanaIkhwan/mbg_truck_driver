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

    // Dipanggil TutorialManager setelah tutorial selesai (New Game)
    public void StartMission()
    {
        currentDelivery = 0;
        totalWaktu = 0f;
        skor = 0;
        StartMissionInternal();
    }

    // Dipanggil saat Continue dari MainMenu
    public void ContinueMission()
    {
        currentDelivery = SaveManager.LoadDelivery();
        totalWaktu = SaveManager.LoadWaktu();
        skor = SaveManager.LoadSkor();

        // Pastikan mission panel aktif
        if (missionPanel != null) missionPanel.SetActive(true);

        StartMissionInternal();
        Debug.Log($"[DeliveryManager] Continue dari delivery ke-{currentDelivery}");
    }

    void StartMissionInternal()
    {
        missionActive = true;
        missionPanel.SetActive(true);
        arrowIndicator.gameObject.SetActive(true);
        if (hudManager != null) hudManager.StartHUD();
        UpdateMissionUI();
    }

    void Update()
    {
        if (missionActive)
            totalWaktu += Time.deltaTime;

        if (!missionActive) return;
        if (currentDelivery >= deliveryPoints.Length) return;
        if (arrowIndicator == null) return;

        Vector3 targetPos = deliveryPoints[currentDelivery].position;
        Vector3 truckPos = truck.position;
        Vector3 direction = targetPos - truckPos;
        direction.y = 0;

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        angle -= truck.eulerAngles.y;
        arrowIndicator.rotation = Quaternion.Euler(0, 0, -angle);
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

            // Hapus save karena misi sudah selesai
            SaveManager.DeleteSave();

            if (misiSelesaiManager != null)
                misiSelesaiManager.ShowMisiSelesai(currentDelivery, totalWaktu, skor);
        }
        else
        {
            // Simpan progress setiap delivery selesai
            SaveManager.SaveProgress(currentDelivery, skor, totalWaktu);

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