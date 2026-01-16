using UnityEditor.EditorTools;
using UnityEngine;

[CreateAssetMenu(menuName = "Car/SuspensionData", fileName = "SuspensionData")]
public class SuspensionData : ScriptableObject
{
    [Header("Geometry")]
    [Tooltip(
        "The distance between the car body and wheel when suspension is fully relaxed(noload)."
    )]
    public float restLength;

    [Tooltip("Maximum distance the suspension can compress from its rest length.")]
    public float maxCompression;

    [Tooltip("Maximum distance the suspension can extend beyong its rest length (wheel dropping).")]
    public float maxExtension;

    [Header("Spring Properties")]
    [Tooltip("How stiff the spring is:: Higher value = less compression for the same load.")]
    public float springStiffness;

    [Tooltip("Resistance applied by the damper to control the suspension velocity.")]
    public float dampingCoefficient;
}
