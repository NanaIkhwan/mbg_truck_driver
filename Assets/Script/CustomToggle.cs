using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomToggle : MonoBehaviour
{
    [Header("Komponen")]
    public Toggle toggle;
    public RectTransform handle;
    public Image background;

    [Header("Posisi Handle")]
    public float posOff = -20f;  // posisi handle saat OFF
    public float posOn  =  20f;  // posisi handle saat ON

    [Header("Warna Background")]
    public Color colorOff = new Color(0.5f, 0.5f, 0.5f); // abu saat OFF
    public Color colorOn  = new Color(0.29f, 0.48f, 1f);  // biru saat ON

    [Header("Kecepatan Animasi")]
    public float smoothSpeed = 8f;

    private float targetX;
    private Color targetColor;

    void Start()
    {
        // Set posisi awal sesuai kondisi toggle
        bool isOn = toggle.isOn;
        targetX     = isOn ? posOn : posOff;
        targetColor = isOn ? colorOn : colorOff;

        // Langsung snap ke posisi awal tanpa animasi
        handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
        if (background != null) background.color = targetColor;

        // Dengarkan perubahan toggle
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        targetX     = isOn ? posOn : posOff;
        targetColor = isOn ? colorOn : colorOff;
    }

    void Update()
    {
        // Smooth geser handle
        float currentX = handle.anchoredPosition.x;
        float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * smoothSpeed);
        handle.anchoredPosition = new Vector2(newX, handle.anchoredPosition.y);

        // Smooth ganti warna background
        if (background != null)
            background.color = Color.Lerp(background.color, targetColor, Time.deltaTime * smoothSpeed);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}