using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Car/CarData", fileName = "CarData")]
public class CarData : ScriptableObject
{
    [Header("Car Body")]
    public float carBody_Mass;
    public Vector3 Car_centerOfMass;

    [Header("Wheels")]
    public WheelData[] ScriptableObjectOf_WheelData;
    public int Xwheeler = 4;
    public int carRadius;
    public int carHeight;
    // reduced from implicit 1.0
}
