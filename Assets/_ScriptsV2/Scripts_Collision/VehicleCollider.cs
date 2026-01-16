using UnityEngine;

[System.Serializable]
public struct OrientedBoundingBox
{
    public Vector3 Center;
    public Vector3 HalfExtents;
    public Quaternion Orientation;

    public OrientedBoundingBox(Vector3 center, Vector3 halfExtents, Quaternion orientation)
    {
        Center = center;
        HalfExtents = halfExtents;
        Orientation = orientation;
    }

    public Vector3[] GetCorners()
    {
        Vector3[] corners = new Vector3[8];
        Vector3 right = Orientation * Vector3.right;
        Vector3 up = Orientation * Vector3.up;
        Vector3 forward = Orientation * Vector3.forward;

        // All 8 corners of the OBB
        corners[0] = Center + right * HalfExtents.x + up * HalfExtents.y + forward * HalfExtents.z;
        corners[1] = Center - right * HalfExtents.x + up * HalfExtents.y + forward * HalfExtents.z;
        corners[2] = Center + right * HalfExtents.x - up * HalfExtents.y + forward * HalfExtents.z;
        corners[3] = Center + right * HalfExtents.x + up * HalfExtents.y - forward * HalfExtents.z;
        corners[4] = Center - right * HalfExtents.x - up * HalfExtents.y + forward * HalfExtents.z;
        corners[5] = Center - right * HalfExtents.x + up * HalfExtents.y - forward * HalfExtents.z;
        corners[6] = Center + right * HalfExtents.x - up * HalfExtents.y - forward * HalfExtents.z;
        corners[7] = Center - right * HalfExtents.x - up * HalfExtents.y - forward * HalfExtents.z;

        return corners;
    }

    public Bounds ToAABB()
    {
        Vector3[] corners = GetCorners();
        Vector3 min = corners[0];
        Vector3 max = corners[0];
        foreach (var c in corners)
        {
            min = Vector3.Min(min, c);
            max = Vector3.Max(max, c);
        }
        return new Bounds((min + max) * 0.5f, max - min);
    }
}

[RequireComponent(typeof(MeshFilter))]
public class VehicleCollider : MonoBehaviour
{
    [Header("Collider Settings")]
    public bool autoGenerateFromMesh = true;
    public bool visualizeCollider = true;
    public Vector3 manualHalfExtents = Vector3.zero;
    public Vector3 manualCenterOffset = Vector3.zero;

    private MeshFilter meshFilter;
    private OrientedBoundingBox obb;

    public OrientedBoundingBox OBB => obb;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        InitializeOBB();
    }

    void InitializeOBB()
    {
        if (autoGenerateFromMesh && meshFilter != null)
        {
            Bounds localBounds = meshFilter.sharedMesh.bounds;
            obb = new OrientedBoundingBox(
                localBounds.center,
                localBounds.extents,
                transform.rotation
            );
        }
        else
        {
            obb = new OrientedBoundingBox(
                manualCenterOffset,
                manualHalfExtents,
                transform.rotation
            );
        }
    }

    void LateUpdate()
    {
        // Update world-space OBB
        obb.Center = transform.TransformPoint(
            autoGenerateFromMesh ? meshFilter.sharedMesh.bounds.center : manualCenterOffset
        );

        obb.Orientation = transform.rotation;

        // If mesh is scaled
        Vector3 scaledExtents = Vector3.Scale(
            autoGenerateFromMesh ? meshFilter.sharedMesh.bounds.extents : manualHalfExtents,
            transform.lossyScale
        );
        obb.HalfExtents = scaledExtents;
    }

    // Optional debug visualization
    void OnDrawGizmos()
    {
        if (!visualizeCollider)
            return;

        var corners = OBB.GetCorners();
        Gizmos.color = Color.yellow;

        // Draw edges
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }
    }
}
