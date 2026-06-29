using UnityEngine;

public class MobileInput : MonoBehaviour
{
    // Nilai ini dibaca oleh mobil.cs sebagai pengganti Input.GetAxis
    public static float verticalInput = 0f;
    public static bool isBraking = false;

    // Dipanggil saat pedal Gas ditekan/dilepas
    public void OnGasDown()  => verticalInput = 1f;
    public void OnGasUp()    => verticalInput = 0f;

    // Dipanggil saat pedal Mundur ditekan/dilepas
    public void OnReverseDown() => verticalInput = -1f;
    public void OnReverseUp()   => verticalInput = 0f;

    // Dipanggil saat pedal Rem ditekan/dilepas
    public void OnBrakeDown() => isBraking = true;
    public void OnBrakeUp()   => isBraking = false;
}