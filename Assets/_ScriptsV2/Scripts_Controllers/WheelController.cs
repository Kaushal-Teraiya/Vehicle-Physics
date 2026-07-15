using Unity.Mathematics;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    //[SerializeField]
    public WheelData wheelData { get; set; }
    private Transform wheelTransform;
    private Vector3 Wheel_InitialPosition_local;

    [SerializeField]
    private float correction_Factor = 0.02f;

    private float suspension_RestLength;
    private float spring_Stiffness;
    private float damperCoefficient;
    private float suspension_maxCompression;
    private float suspension_maxExtension;
    private float wheelRadius;

    private float currentCompression;
    private float lastCompression;

    public bool Wheel_isOnGround { get; private set; }
    public Vector3 contactPointOf_WheelOnGround { get; private set; }
    public bool ShowRayCastGizmos;
    public Vector3 wheelForce { get; private set; }

    public void Initialize(WheelData _wheelData)
    {
        if (_wheelData == null || _wheelData.DataOf_Suspension == null)
        {
            Debug.LogError("WheelData or SuspensionData not assigned properly.");
            return;
        }

        wheelData = _wheelData;
        wheelTransform = transform.GetChild(0);
        wheelRadius = wheelData.RasiusOf_Wheel;
        Wheel_InitialPosition_local = wheelTransform.localPosition;

        var Suspension = wheelData.DataOf_Suspension;

        suspension_RestLength = Suspension.restLength;
        spring_Stiffness = Suspension.springStiffness;
        damperCoefficient = Suspension.dampingCoefficient;
        suspension_maxCompression = Suspension.maxCompression;
        suspension_maxExtension = Suspension.maxExtension;

        // start with wheels fully drooped
        currentCompression = -suspension_maxExtension;
        lastCompression = currentCompression;
    }

    public void SimulateWheel(float dt_fixedDeltaTime, Transform carBodyTransform)
    {
        RaycastHit hitInfo;

        Vector3 rayOrigin = transform.position;
        Vector3 rayDir = -transform.up;
        float rayLength = suspension_RestLength + suspension_maxExtension + wheelRadius + 0.01f;

        Wheel_isOnGround = Physics.Raycast(rayOrigin, rayDir, out hitInfo, rayLength);

        if (Wheel_isOnGround)
        {
            contactPointOf_WheelOnGround = hitInfo.point;

            // Measure total distance from suspension pivot to wheel contact (minus radius)
            float suspensionLength = hitInfo.distance - wheelRadius - correction_Factor;
            float rawCompression = suspension_RestLength - suspensionLength;

            // Clamp within limits
            currentCompression = Mathf.Clamp(
                rawCompression,
                -suspension_maxExtension,
                suspension_maxCompression
            );

            // Calculate forces
            float springForce = spring_Stiffness * currentCompression;
            float WheelDisplacement = currentCompression - lastCompression;
            float suspensionVelocity = WheelDisplacement / dt_fixedDeltaTime;
            float damperForce = damperCoefficient * suspensionVelocity;

            wheelForce = wheelTransform.up * (springForce + damperForce);
            lastCompression = currentCompression;
        }
        else
        {
            currentCompression = -suspension_maxExtension;
            wheelForce = Vector3.zero;
        }

        //The above if else block was just for determining the compression amount , the application of this value is in the below function

        UpdateWheelPosition(carBodyTransform, dt_fixedDeltaTime);
    }

    // public void SimulateWheel(float dt_fixedDeltaTime, Transform carBodyTransform)
    // {
    //     RaycastHit hitInfo;
    //     Vector3 rayOrigin = transform.position;
    //     Vector3 rayDir = -transform.up;
    //     float rayLength = suspension_RestLength + suspension_maxExtension + wheelRadius + 0.01f;

    //     Wheel_isOnGround = Physics.Raycast(rayOrigin, rayDir, out hitInfo, rayLength);

    //     if (Wheel_isOnGround)
    //     {
    //         contactPointOf_WheelOnGround = hitInfo.point;
    //         float suspensionLength = hitInfo.distance - wheelRadius - correction_Factor;
    //         float rawCompression = suspension_RestLength - suspensionLength;

    //         currentCompression = Mathf.Clamp(rawCompression, -suspension_maxExtension, suspension_maxCompression);

    //         float springForce = spring_Stiffness * currentCompression;
    //         float WheelDisplacement = rawCompression - lastCompression; // unclamped delta — fixed earlier in thread
    //         float suspensionVelocity = WheelDisplacement / dt_fixedDeltaTime;
    //         float damperForce = damperCoefficient * suspensionVelocity;

    //         wheelForce = wheelTransform.up * (springForce + damperForce);
    //         lastCompression = rawCompression;
    //     }
    //     else
    //     {
    //         currentCompression = -suspension_maxExtension;
    //         wheelForce = Vector3.zero;
    //     }

    //     UpdateWheelPosition(carBodyTransform, dt_fixedDeltaTime);
    // }

    private void UpdateWheelPosition(Transform carBodyTransform, float dt)
    {
        float effectiveLength = suspension_RestLength - currentCompression;
        effectiveLength = Mathf.Clamp(
            effectiveLength,
            suspension_RestLength - suspension_maxCompression,
            suspension_RestLength + suspension_maxExtension
        );

        // Apply compression relative to the wheel’s initial local rest position
        Vector3 localTarget = Vector3.down * effectiveLength;

        float lerpSpeed = Wheel_isOnGround ? 15f : 5f;
        wheelTransform.localPosition = Vector3.Lerp(
            wheelTransform.localPosition,
            localTarget,
            dt * lerpSpeed
        );

        float visualCompression = currentCompression * 4f; // exaggerate compression 4x for visuals

        float compressionRatio = Mathf.InverseLerp(
            -suspension_maxExtension,
            suspension_maxCompression,
            visualCompression
        );

        float tiltAngle = Mathf.Lerp(-5f, 5f, compressionRatio);
        float sideSign = transform.localPosition.x > 0f ? 1f : -1f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle * sideSign);

        wheelTransform.localRotation = Quaternion.Lerp(
            wheelTransform.localRotation,
            targetRot,
            dt * 10f
        );

        // Debug.Log($"Wheel {name} Compression: {currentCompression:F3}");
    }

    public float GetSuspensionCompressionAmount() => Mathf.Max(0f, currentCompression);

    public float GetRestLength() => suspension_RestLength;

    public float GetCurrentCompression() => Mathf.Max(0f, currentCompression);
    public float GetWheelRadius() => wheelRadius;

    public float GetFrictionCoefficient()
    {
        return Wheel_isOnGround ? wheelData.frictionCoefficient : 0f;
    }

    void OnDrawGizmos()
    {
        if (!ShowRayCastGizmos)
            return;

        float rayLength = suspension_RestLength + suspension_maxExtension + wheelRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            wheelTransform.position,
            wheelTransform.position - wheelTransform.up * rayLength
        );
    }
}
