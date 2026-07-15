using UnityEngine;

[CreateAssetMenu(menuName = "Car/WheelData", fileName = "WheelData")]
public class WheelData : ScriptableObject
{
    public string NameOf_Wheel = "wheel";
    public float RasiusOf_Wheel;
    public float MassOf_Wheel;
    public SuspensionData DataOf_Suspension;

    [Range(0f, 1f)]
    public float frictionCoefficient = 0.8f;
    public float wheelCorrectionFactor = 0.1f;
}
