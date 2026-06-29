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
    public float rotateSpeed  = 90f;     // derajat per detik
    public Vector3 rotateAxis = Vector3.up; // sumbu rotasi, bisa diubah di Inspector

    [Header("Audio")]
    public AudioClip checkpointSound;
    public AudioClip missionCompleteSound;

    private Vector3 arrowStartPos;
    private Quaternion arrowStartRot;    // simpan rotasi awal dari Inspector
    private AudioSource audioSource;
    private bool alreadyTriggered = false;
    private float currentAngle = 0f;

    void Start()
    {
        if (arrowObject != null)
        {
            arrowStartPos = arrowObject.localPosition;
            arrowStartRot = arrowObject.localRotation; // simpan rotasi awal
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (arrowObject == null) return;
        if (alreadyTriggered) return;

        // Animasi bounce naik turun
        float newY = arrowStartPos.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        arrowObject.localPosition = new Vector3(
            arrowStartPos.x,
            newY,
            arrowStartPos.z
        );

        // Rotasi berputar dari rotasi awal Inspector
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