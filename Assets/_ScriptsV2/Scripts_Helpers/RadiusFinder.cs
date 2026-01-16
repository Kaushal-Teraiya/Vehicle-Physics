using UnityEngine;

public class RadiusFinder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Vector3[] verts = mesh.vertices;

        float minY = float.MaxValue;
        foreach (var v in verts)
        {
            if (v.y < minY)
            {
                minY = v.y;
                Debug.Log("RADIUS :-" + Mathf.Abs(minY) * transform.localScale.y);
            }
        }
    }

    // Update is called once per frame
    void Update() { }
}
