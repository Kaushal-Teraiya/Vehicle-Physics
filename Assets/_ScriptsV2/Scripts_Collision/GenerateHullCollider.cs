using UnityEngine;

public class GenerateConvexCollider : MonoBehaviour
{
    public bool generateAtStart = true;
    public Mesh colliderMesh;

    void Start()
    {
        if (!generateAtStart)
            return;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.LogError("No MeshFilter found!");
            return;
        }

        colliderMesh = ConvexHullGenerator.GenerateHull(mf.sharedMesh);

        // Visualize
        GameObject hullObj = new GameObject("ConvexHull");
        hullObj.transform.SetParent(transform);
        hullObj.transform.localPosition = Vector3.zero;
        hullObj.transform.localRotation = Quaternion.identity;

        MeshFilter hullMF = hullObj.AddComponent<MeshFilter>();
        hullMF.mesh = colliderMesh;

        MeshRenderer mr = hullObj.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));
        mr.material.color = new Color(0, 1, 0, 0.3f);
    }
}
