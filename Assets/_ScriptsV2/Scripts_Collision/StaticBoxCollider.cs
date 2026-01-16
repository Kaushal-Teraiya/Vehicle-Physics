using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class StaticBoxCollider : MonoBehaviour
{
    public bool autoFromMesh = true;
    public Vector3 manualHalfExtents = Vector3.zero;
    public Vector3 manualCenterOffset = Vector3.zero;
    public bool visualize = true;

    private MeshFilter meshFilter;
    private OrientedBoundingBox obb;

    public OrientedBoundingBox OBB => obb;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        Bounds localBounds = meshFilter.sharedMesh.bounds;

        if (autoFromMesh)
        {
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
        obb.Center = transform.TransformPoint(
            autoFromMesh ? meshFilter.sharedMesh.bounds.center : manualCenterOffset
        );

        obb.Orientation = transform.rotation;

        obb.HalfExtents = Vector3.Scale(
            autoFromMesh ? meshFilter.sharedMesh.bounds.extents : manualHalfExtents,
            transform.lossyScale
        );
    }

    void OnDrawGizmos()
    {
        if (!visualize)
            return;

        Vector3[] corners = OBB.GetCorners();
        Gizmos.color = Color.green;

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            Gizmos.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]);
            Gizmos.DrawLine(corners[i], corners[i + 4]);
        }
    }
}
