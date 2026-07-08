using UnityEngine;

public class DropZone : MonoBehaviour
{
    [Header("Referensi")]
    public DeliveryManager deliveryManager;
    public string sekolahName = "SDN Ambarusdi";

    [Header("Animasi Panah")]
    public Transform arrowObject;
    public float bounceHeight = 0.3f;
    public float bounceSpeed  = 2f;
    public float rotateSpeed  = 90f;
    public Vector3 rotateAxis = Vector3.up;

    [Header("Audio")]
    public AudioClip checkpointSound;
    public AudioClip missionCompleteSound;

    private Vector3 arrowStartPos;
    private Quaternion arrowStartRot;
    private AudioSource audioSource;
    private bool alreadyTriggered = false;
    private float currentAngle = 0f;

    void Start()
    {
        if (arrowObject != null)
        {
            arrowStartPos = arrowObject.localPosition;
            arrowStartRot = arrowObject.localRotation;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // Sync volume dengan SettingManager
        audioSource.volume = SettingManager.SFXVolume;
    }

    void Update()
    {
        // Sync volume tiap frame
        audioSource.volume = SettingManager.SFXVolume;

        if (arrowObject == null) return;
        if (alreadyTriggered) return;

        float newY = arrowStartPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        arrowObject.localPosition = new Vector3(arrowStartPos.x, newY, arrowStartPos.z);

        currentAngle += rotateSpeed * Time.deltaTime;
        arrowObject.localRotation = arrowStartRot * Quaternion.AngleAxis(currentAngle, rotateAxis);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Truck")) return;
        if (alreadyTriggered) return;

        alreadyTriggered = true;

        if (arrowObject != null)
            arrowObject.gameObject.SetActive(false);

        bool isLastDelivery = deliveryManager.IsLastDelivery();

        if (isLastDelivery && missionCompleteSound != null)
            audioSource.PlayOneShot(missionCompleteSound);
        else if (checkpointSound != null)
            audioSource.PlayOneShot(checkpointSound);

        deliveryManager.OnDeliveryComplete(sekolahName);
    }

    public void ResetDropZone()
    {
        alreadyTriggered = false;
        currentAngle = 0f;
        if (arrowObject != null)
        {
            arrowObject.gameObject.SetActive(true);
            arrowObject.localPosition = arrowStartPos;
            arrowObject.localRotation = arrowStartRot;
        }
    }
}