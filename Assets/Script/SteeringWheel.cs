using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SteeringWheel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Setting")]
    public float maxRotationAngle = 60f;
    public float returnSpeed = 8f;
    public float smoothSpeed = 15f;

    [Header("Referensi")]
    public mobil truckMobil;

    private RectTransform rectTransform;
    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private float lastTouchAngle = 0f;
    private bool isDragging = false;

    [HideInInspector] public bool isSteeringLeft  = false;
    [HideInInspector] public bool isSteeringRight = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isDragging)
        {
            float keyInput = Input.GetAxis("Horizontal");

            if (Mathf.Abs(keyInput) > 0.01f)
            {
                targetAngle = -keyInput * maxRotationAngle;
                truckMobil.SetSteerInput(keyInput);

                // Deteksi arah dari keyboard
                isSteeringLeft  = keyInput < -0.1f;
                isSteeringRight = keyInput >  0.1f;
            }
            else
            {
                targetAngle = Mathf.Lerp(targetAngle, 0f, Time.deltaTime * returnSpeed);
                float steerValue = -(currentAngle / maxRotationAngle);
                truckMobil.SetSteerInput(steerValue);

                isSteeringLeft  = false;
                isSteeringRight = false;
            }
        }

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastTouchAngle = GetAngleFromCenter(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float touchAngle = GetAngleFromCenter(eventData.position);
        float deltaAngle = Mathf.DeltaAngle(lastTouchAngle, touchAngle);
        lastTouchAngle = touchAngle;

        targetAngle = Mathf.Clamp(
            targetAngle + deltaAngle,
            -maxRotationAngle,
            maxRotationAngle
        );

        float steerValue = -(targetAngle / maxRotationAngle);
        truckMobil.SetSteerInput(steerValue);

        // Deteksi arah dari steering wheel
        isSteeringRight = targetAngle < -5f;
        isSteeringLeft  = targetAngle >  5f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        isSteeringLeft  = false;
        isSteeringRight = false;
        truckMobil.SetSteerInput(0f);
    }

    float GetAngleFromCenter(Vector2 touchPosition)
    {
        Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(
            null, rectTransform.position
        );

        Vector2 direction = touchPosition - centerScreen;
        return Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
    }
}