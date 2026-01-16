using UnityEngine;

public static class CollisionMath
{
    // SAT-based OBB vs OBB
    public static bool OBBvsOBB(
        OrientedBoundingBox a,
        OrientedBoundingBox b,
        out Vector3 normal,
        out float penetration
    )
    {
        normal = Vector3.zero;
        penetration = float.MaxValue;

        // Local axes
        Vector3[] axesA =
        {
            a.Orientation * Vector3.right,
            a.Orientation * Vector3.up,
            a.Orientation * Vector3.forward,
        };

        Vector3[] axesB =
        {
            b.Orientation * Vector3.right,
            b.Orientation * Vector3.up,
            b.Orientation * Vector3.forward,
        };

        // 15 axes to test (6 faces + 9 cross products)
        Vector3[] axesToTest = new Vector3[15];
        int index = 0;

        for (int i = 0; i < 3; i++)
        {
            axesToTest[index++] = axesA[i];
            axesToTest[index++] = axesB[i];
        }

        for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
            axesToTest[index++] = Vector3.Cross(axesA[i], axesB[j]).normalized;

        // Center difference
        Vector3 centerDiff = b.Center - a.Center;

        bool collided = true;

        foreach (var axis in axesToTest)
        {
            if (axis == Vector3.zero)
                continue;

            float projectionA = ProjectOBB(a, axis);
            float projectionB = ProjectOBB(b, axis);

            float distance = Mathf.Abs(Vector3.Dot(centerDiff, axis));
            float overlap = projectionA + projectionB - distance;

            if (overlap < 0f)
            {
                collided = false;
                break;
            }

            if (overlap < penetration)
            {
                penetration = overlap;
                normal = axis;
            }
        }

        // Make sure normal points from ground → car
        if (Vector3.Dot(normal, centerDiff) < 0f)
            normal = -normal;

        return collided;
    }

    private static float ProjectOBB(OrientedBoundingBox obb, Vector3 axis)
    {
        axis = axis.normalized;

        float r =
            Mathf.Abs(Vector3.Dot(axis, obb.Orientation * Vector3.right)) * obb.HalfExtents.x
            + Mathf.Abs(Vector3.Dot(axis, obb.Orientation * Vector3.up)) * obb.HalfExtents.y
            + Mathf.Abs(Vector3.Dot(axis, obb.Orientation * Vector3.forward)) * obb.HalfExtents.z;

        return r;
    }
}
