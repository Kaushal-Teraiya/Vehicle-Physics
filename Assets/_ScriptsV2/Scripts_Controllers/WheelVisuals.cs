using UnityEngine;

public class WheelVisuals : MonoBehaviour
{
    private WheelController wheel;


    private void Awake()
    {
        wheel = GetComponentInChildren<WheelController>();
    }
    public void SimulateWheelVisual(float dt)
    {
        UpdateWheelSpin(dt);
        UpdateSuspension(dt);
        UpdateCamberTilt(dt);
        UpdateSteering(dt);
    }

    private void UpdateSuspension(float dt)
    {
        float effectiveLength = wheel.suspension_RestLength - wheel.currentCompression;
        effectiveLength = Mathf.Clamp(
            effectiveLength,
            wheel.suspension_RestLength - wheel.suspension_maxCompression,
            wheel.suspension_RestLength + wheel.suspension_maxExtension
        );

        // Apply compression relative to the wheel’s initial local rest position
        Vector3 localTarget = Vector3.down * effectiveLength;

        float lerpSpeed = wheel.Wheel_isOnGround ? 15f : 5f;
        wheel.wheelTransform.localPosition = Vector3.Lerp(
            wheel.wheelTransform.localPosition,
            localTarget,
            dt * lerpSpeed
        );

    }

    private void UpdateCamberTilt(float dt)
    {
        float visualCompression = wheel.currentCompression * 4f; // exaggerate compression 4x for visuals

        float compressionRatio = Mathf.InverseLerp(
            -wheel.suspension_maxExtension,
            wheel.suspension_maxCompression,
            visualCompression
        );
        float tiltAngle = Mathf.Lerp(-5f, 5f, compressionRatio);
        float sideSign = transform.localPosition.x > 0f ? 1f : -1f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle * sideSign);

        wheel.wheelTransform.localRotation = Quaternion.Lerp(
            wheel.wheelTransform.localRotation,
            targetRot,
            dt * 10f
        );
    }

    private void UpdateSteering(float dt)
    {

    }

    private void UpdateWheelSpin(float dt)
    {
        float angle = wheel.WheelAngularVelocity * Mathf.Rad2Deg * dt;
        wheel.WheelTransform.Rotate(Vector3.right, angle, Space.Self);
    }
}
