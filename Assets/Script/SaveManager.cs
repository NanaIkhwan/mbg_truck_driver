using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Key untuk PlayerPrefs
    private const string KEY_DELIVERY   = "CurrentDelivery";
    private const string KEY_SKOR       = "Skor";
    private const string KEY_WAKTU      = "TotalWaktu";
    private const string KEY_HAS_SAVE   = "HasSave";

    public static void SaveProgress(int currentDelivery, int skor, float waktu)
    {
        PlayerPrefs.SetInt(KEY_DELIVERY, currentDelivery);
        PlayerPrefs.SetInt(KEY_SKOR, skor);
        PlayerPrefs.SetFloat(KEY_WAKTU, waktu);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] Progress disimpan: delivery {currentDelivery}, skor {skor}");
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(KEY_DELIVERY);
        PlayerPrefs.DeleteKey(KEY_SKOR);
        PlayerPrefs.DeleteKey(KEY_WAKTU);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 0);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Save dihapus");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    }

    public static int LoadDelivery()  => PlayerPrefs.GetInt(KEY_DELIVERY, 0);
    public static int LoadSkor()      => PlayerPrefs.GetInt(KEY_SKOR, 0);
    public static float LoadWaktu()   => PlayerPrefs.GetFloat(KEY_WAKTU, 0f);
}