using UnityEngine;

public class GenerateHull3D : MonoBehaviour
{
    public Mesh hull;

    void Start()
    {
        var mf = GetComponent<MeshFilter>();
        hull = QuickHull3D.GenerateHull(mf.sharedMesh.vertices);

        GameObject h = new GameObject("HULL");
        h.transform.SetParent(transform);
        h.transform.localPosition = Vector3.zero;

        var mf2 = h.AddComponent<MeshFilter>();
        var mr2 = h.AddComponent<MeshRenderer>();

        mf2.mesh = hull;
        mr2.material = new Material(Shader.Find("Standard"))
        {
            color = new Color(0, 1, 0, 0.3f)
        };
    }
}
