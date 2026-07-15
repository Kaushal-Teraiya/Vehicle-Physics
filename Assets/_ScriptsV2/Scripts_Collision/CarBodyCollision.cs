using UnityEngine;

public class CarBodyCollider
{
    public Vector3 localCenter;  // offset from car center
    public float radius;         // collision radius
    public float halfHeight;     // half the capsule height

    public CarBodyCollider(Vector3 center, float radius, float height)
    {
        localCenter = center;
        this.radius = radius;
        halfHeight = height * 0.5f;
    }

    public Vector3 GetWorldCenter(Transform bodyTransform)
    {
        return bodyTransform.TransformPoint(localCenter);
    }

    public void GetCapsulePoints(Transform bodyTransform, out Vector3 p1, out Vector3 p2)
    {
        Vector3 worldCenter = GetWorldCenter(bodyTransform);
        Vector3 upDir = bodyTransform.up;

        p1 = worldCenter - upDir * halfHeight;
        p2 = worldCenter + upDir * halfHeight;
    }
}