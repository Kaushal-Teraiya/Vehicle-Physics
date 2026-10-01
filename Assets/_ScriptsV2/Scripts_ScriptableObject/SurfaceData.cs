
using UnityEngine;

[CreateAssetMenu(menuName = "Surface/SurfaceData", fileName = "SurfaceData")]
public class SurfaceData : ScriptableObject
{
    [Range(0f, 2f)]
    public float frictionCoefficient = 0.9f;
}