using UnityEngine;

[CreateAssetMenu(fileName = "CarSettings", menuName = "Car/Settings")]
public class CarSettings : ScriptableObject
{
    [Header("Movement")]
    public float acceleration = 20f;
    public float maxSpeed = 50f;
    public float deceleration = 30f;
    public float decelerationToRest = 2f;
    public float turnSpeed = 100f;

    [Header("Suspension")]
    public float suspensionRestLength = 0.4f;
    public float suspensionRange = 0.3f;
    public float suspensionStrength = 8f;
    public float suspensionDamping = 6f;
    public float compressionMultiplier = 0.1f;
    public float YoriginOffset = 0f;
    public float wheelRadius = 0.35f;

    [Header("Collision")]
    public float collisionRayLength = 1f;

    [Header("Steering")]
    public float maxSteerAngle = 30f;

    [Header("Camera Shake")]
    public float baseShakeIntensity = 0.03f;
    public float speedShakeMultiplier = 0.002f;
    public float bumpShakeIntensity = 0.05f;
    public float shakeDamping = 5f;
    public float wheelRotationMultiplier = 360f;
}
