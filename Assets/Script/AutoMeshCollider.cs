using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AutoMeshCollider : MonoBehaviour
{
    void Start()
    {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

        MeshFilter meshFilter = GetComponent<MeshFilter>();

        meshCollider.sharedMesh = meshFilter.sharedMesh;

        // Opsional
        meshCollider.convex = false;
    }
}