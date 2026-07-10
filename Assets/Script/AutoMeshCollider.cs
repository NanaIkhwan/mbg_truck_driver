using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AutoMeshCollider : MonoBehaviour
{
    [Tooltip("Centang jika object ini bergerak/punya Rigidbody")]
    public bool isMovingObject = false;

    [Tooltip("Paksa convex meski object statis (lebih performa)")]
    public bool forceConvex = false;

    void Awake() // Awake lebih awal dari Start, lebih aman di Android
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning($"[AutoMeshCollider] Mesh tidak ditemukan di {gameObject.name}");
            return;
        }

        // Hapus collider lama kalau ada
        MeshCollider existing = GetComponent<MeshCollider>();
        if (existing != null) DestroyImmediate(existing);

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        // Object bergerak WAJIB convex
        // Object statis bisa non-convex (lebih presisi)
        meshCollider.convex = isMovingObject || forceConvex;

        // Pastikan object statis tidak punya Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !isMovingObject)
        {
            rb.isKinematic = true;
            Debug.LogWarning($"[AutoMeshCollider] {gameObject.name} punya Rigidbody tapi bukan moving object — di-set Kinematic");
        }

        Debug.Log($"[AutoMeshCollider] {gameObject.name} → convex: {meshCollider.convex}");
    }
}