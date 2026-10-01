using UnityEngine;

[CreateAssetMenu(menuName = "Car/WheelData", fileName = "WheelData")]
public class WheelData : ScriptableObject
{
    public string NameOf_Wheel = "wheel";
    public float RasiusOf_Wheel;
    public float MassOf_Wheel;
    public SuspensionData DataOf_Suspension;
    public AnimationCurve tractionCurve;
    public float wheelCorrectionFactor = 0.1f;
    public bool isDriven;
    public bool canSteer;
    [Header("Longitudinal Pacejka")]
    public float LongiPacejkaB = 10.0f; //Stiffness
    public float LongiPacejkaC = 1.9f; //Shape
    public float LongiPacejkaE = 0.97f; //curvature

    [Header("Lateral Pacejka")]
    public float LatPacejkaB = 10.0f;
    public float LatPacejkaC = 1.3f;
    public float LatPacejkaD;  //friction coefficeient of current surface * normal force ; calculated directly in wheel controllers
    public float LatPacejkaE = 0.97f;
}
