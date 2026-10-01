using System;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;

public class WheelController : MonoBehaviour
{
    [SerializeField] private float rollingResistanceCoefficient = 0.015f;
    private float driveTorque;
    //[SerializeField] private AnimationCurve tractionCurve;
    public WheelData wheelData { get; set; }
    public Transform wheelTransform { get; private set; }
    private Vector3 Wheel_InitialPosition_local;

    [SerializeField]
    private float correction_Factor = 0.02f;

    public float suspension_RestLength { get; private set; }
    private float spring_Stiffness;
    private float damperCoefficient;
    public float suspension_maxCompression { get; private set; }
    public float suspension_maxExtension { get; private set; }
    public float wheelRadius { get; private set; }
    private WheelVisuals wheelVisuals;
    public float currentCompression { get; private set; }
    public float WheelAngularVelocity => wheelPhysics.GetAngularVelocity();
    public Transform WheelTransform => wheelTransform;
    //private float lastCompression;

    public bool Wheel_isOnGround { get; private set; }
    public Vector3 contactPointOf_WheelOnGround { get; private set; }
    public bool ShowRayCastGizmos;
    private RaycastHit hitInfo;
    private Vector3 lateralForce; //Along wheel right

    public Vector3 wheelForce { get; private set; }
    private WheelPhysics wheelPhysics;
    private float currentSurfaceFrictionCoefficient;
    public Vector3 suspensionForce;
    private float netTorque;
    private Vector3 LongitudinalForce; //Along wheel forward 
    private float slipAngleDebug;
    public float steeringAngle { get; private set; }

    private void Awake()
    {
        wheelVisuals = GetComponent<WheelVisuals>();
    }

    public void Initialize(WheelData _wheelData)
    {
        if (_wheelData == null || _wheelData.DataOf_Suspension == null)
        {
            Debug.LogError("WheelData or SuspensionData not assigned properly.");
            return;
        }
        //=========Initialize Wheel===========//
        wheelData = _wheelData;
        wheelTransform = transform.GetChild(0);
        wheelRadius = wheelData.RasiusOf_Wheel;
        wheelPhysics = new WheelPhysics(wheelData.MassOf_Wheel, wheelRadius);
        Wheel_InitialPosition_local = wheelTransform.localPosition;
        var Suspension = wheelData.DataOf_Suspension;

        //=========Initialize Suspension============//
        suspension_RestLength = Suspension.restLength;
        spring_Stiffness = Suspension.springStiffness;
        damperCoefficient = Suspension.dampingCoefficient;
        suspension_maxCompression = Suspension.maxCompression;
        suspension_maxExtension = Suspension.maxExtension;

        // start with wheels fully drooped
        currentCompression = -suspension_maxExtension;
        //lastCompression = currentCompression;
    }

    public void SteeringAngle(float angle)
    {
        steeringAngle = angle;
    }
    public void SimulateWheel(float dt_fixedDeltaTime, Transform carBodyTransform, Vector3 chassisVelocity, Vector3 chassisAngularVelocity)
    {
        RaycastHit hitInfo;
        hitInfo = default;
        Vector3 rayOrigin = transform.position + transform.up * wheelRadius;

        Vector3 rayDir = -transform.up;
        float rayLength = suspension_RestLength + suspension_maxExtension + wheelRadius + 0.01f;

        RaycastHit[] hits = Physics.SphereCastAll(
                                rayOrigin,
                                wheelRadius,
                                rayDir,
                                rayLength
                            );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Wheel_isOnGround = false;

        foreach (var hit in hits)
        {
            //======Get Current Surface Friction Coefficient=====//
            if (hit.collider.TryGetComponent(out Surface surface))
            {
                currentSurfaceFrictionCoefficient = surface.surfaceData.frictionCoefficient;
            }
            else
            {
                currentSurfaceFrictionCoefficient = 0.8f; // default
            }

            //==== Ignore Lateral Hemisphere Collisions======//
            Vector3 sphereCenter = rayOrigin + rayDir * hit.distance;
            Vector3 sphereContactNormal = (hit.point - sphereCenter).normalized;
            float soleDot = Vector3.Dot(sphereContactNormal, -transform.up);

            if (soleDot < 0.60f)
                continue;

            hitInfo = hit;
            Wheel_isOnGround = true;
            break;
        }

        this.hitInfo = hitInfo;

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

            var steeringRotation = Quaternion.AngleAxis(steeringAngle, transform.up);

            Vector3 wheelForward = steeringRotation * transform.forward;
            Vector3 wheelRight = steeringRotation * transform.right;
            // Calculate forces

            //=========Suspension forces=============//
            float springForce = spring_Stiffness * currentCompression;
            Vector3 r = transform.position - carBodyTransform.position;
            Vector3 wheelPivotVelocity = chassisVelocity + Vector3.Cross(chassisAngularVelocity, r);

            float suspensionVelocity = -Vector3.Dot(wheelPivotVelocity, transform.up);
            float damperForce = damperCoefficient * suspensionVelocity;
            suspensionForce = hitInfo.normal * (springForce + damperForce);

            //=========Wheel Forces=============//
            float wheelSurfaceVelocity = wheelPhysics.GetSurfaceSpeed(GetWheelRadius());
            float longitudinalVelocity = Vector3.Dot(chassisVelocity, wheelForward);

            float referenceSpeed = Mathf.Max(Mathf.Abs(longitudinalVelocity), 0.1f);
            float slipRatio = (wheelSurfaceVelocity - longitudinalVelocity) / referenceSpeed;
            slipRatio = Mathf.Clamp(slipRatio, -3f, 3f);

            //Longitudinal pacejka forces//
            float engineTorque = wheelData.isDriven ? driveTorque : 0f;
            float normalForce = suspensionForce.magnitude;

            float longitudinalPacejkaForce = 0f;

            if (Mathf.Abs(engineTorque) > 0.001f)
            {
                longitudinalPacejkaForce = PacejkaLongitudinal(slipRatio, normalForce, currentSurfaceFrictionCoefficient);
            }

            float targetAngularVelocity = longitudinalVelocity / wheelRadius;
            float angularVelocityError = targetAngularVelocity - wheelPhysics.GetAngularVelocity();

            if (!wheelData.isDriven)
            {
                // Free-rolling wheel follows vehicle speed.
                float freeRollingTorque = angularVelocityError * 20f;
                netTorque = Mathf.Clamp(freeRollingTorque, -150f, 150f);
            }
            else
            {
                float reactionTorque = longitudinalPacejkaForce * wheelRadius;

                // Engine torque + tire reaction torque.
                netTorque = engineTorque - reactionTorque;

                // When throttle is released, let the driven wheel
                // naturally synchronize back to the vehicle speed.
                if (Mathf.Abs(engineTorque) < 0.001f)
                {
                    float synchronizationTorque = angularVelocityError * 20f;
                    netTorque += Mathf.Clamp(synchronizationTorque, -150f, 150f);
                }
            }

            //Lateral Pacejka Forces//
            Vector3 _r = transform.position - carBodyTransform.position;
            Vector3 contactPatchVelocity = chassisVelocity + Vector3.Cross(chassisAngularVelocity, r); // This cross product returns tangential velocity of a wheel at a given instance caused due to the angular velocity of the car chassis

            float wheelForwardVelocity = Vector3.Dot(contactPatchVelocity, wheelForward);
            float wheelLateralVelocity = Vector3.Dot(contactPatchVelocity, wheelRight);

            float slipAngle = Mathf.Atan2(wheelLateralVelocity, Mathf.Abs(wheelForwardVelocity));
            slipAngleDebug = slipAngle;
            float lateralPacejkaForce = PacejkaLateral(slipAngle, normalForce, currentSurfaceFrictionCoefficient);
            float carSpeed = chassisVelocity.magnitude;
            float steeringEffect = Mathf.InverseLerp(0.5f, 2f, carSpeed);
            lateralPacejkaForce *= steeringEffect;
            lateralForce = -wheelRight * lateralPacejkaForce;

            //========stationary lateral grip===========//


            float lateralVelocity = Vector3.Dot(contactPatchVelocity, wheelRight);

            if (Mathf.Abs(lateralVelocity) < 0.2f)
            {
                float tireStiffness = 5000f; // tune

                lateralForce = -wheelRight * lateralVelocity * tireStiffness;

                float maxGrip =
                    currentSurfaceFrictionCoefficient * normalForce;

                if (lateralForce.magnitude > maxGrip)
                    lateralForce =
                        lateralForce.normalized * maxGrip;
            }


            //===========Rolling Resistance============/
            float rollingResistanceForce = 0f;

            if (Mathf.Abs(wheelForwardVelocity) > 0.01f)
            {
                rollingResistanceForce = rollingResistanceCoefficient * normalForce;
                rollingResistanceForce *= Mathf.Sign(wheelForwardVelocity);
            }

            float totalLongitudinalForce = longitudinalPacejkaForce - rollingResistanceForce;
            // float reactionTorque = longitudinalPacejkaForce * GetWheelRadius();
            // netTorque = engineTorque - reactionTorque;
            LongitudinalForce = wheelForward * totalLongitudinalForce;

            //Final Force on wheel
            wheelForce = suspensionForce + LongitudinalForce + lateralForce;

            Debug.Log($"Slip Angle: {Mathf.Rad2Deg * slipAngle:F2}");
            Debug.Log(
                    $"Forward: {wheelForwardVelocity:F2}  " +
                    $"Lateral: {wheelLateralVelocity:F2}  " +
                    $"Slip: {Mathf.Rad2Deg * slipAngle:F1}"
                    );

            Debug.Log("CHASSIS ANGULAR VELOCITY " + chassisAngularVelocity);

            float forwardSpeed = Vector3.Dot(chassisVelocity, transform.forward);
            Vector3 up = transform.up;

            Debug.Log(
                $"up = ({up.x:F6}, {up.y:F6}, {up.z:F6})"
            );
            Debug.Log($"Suspension Force: {suspensionForce}");
            Debug.Log($"Forward Speed: {forwardSpeed:F2} m/s");
            Debug.Log($"Slip Ratio: {slipRatio}");
            Debug.Log($"Longitudinal Force: {longitudinalPacejkaForce}");
            Debug.Log($"Net Torque: {netTorque}");
            Debug.Log($"Wheel Surface Speed: {wheelSurfaceVelocity}");
        }
        else
        {
            currentCompression = -suspension_maxExtension;
            wheelForce = Vector3.zero;
        }

        //The above if else block was just for determining the compression amount , the application of this value is in the below function
        wheelPhysics.IntegrateRotation(dt_fixedDeltaTime, netTorque);
        UpdateWheelVisuals(carBodyTransform, dt_fixedDeltaTime);
        //wheelVisuals?.SimulateWheelVisual(dt_fixedDeltaTime);
        //Debug.Log("Angular velocity" + wheelPhysics.GetAngularVelocity());
        // Debug.Log($"Surface: {wheelSurfaceVelocity:F2}  Vehicle: {longitudinalVelocity:F2}");

    }

    private float PacejkaLongitudinal(float slipRatio, float normalForce, float frictionCoefficient)
    {
        float B = wheelData.LongiPacejkaB;
        float C = wheelData.LongiPacejkaC;
        float E = wheelData.LongiPacejkaE;

        float D = frictionCoefficient * normalForce;

        float Bk = B * slipRatio;

        return D * Mathf.Sin(C * Mathf.Atan(Bk - E * (Bk - Mathf.Atan(Bk))));
    }

    private float PacejkaLateral(float slipAngle, float normalForce, float frictionCoefficient)
    {
        //lateral pacejka magic formula  Fy​=D⋅sin(C⋅arctan(Bα−E(Bα−arctan(Bα))))
        float B = wheelData.LatPacejkaB;
        float C = wheelData.LatPacejkaC;
        float E = wheelData.LatPacejkaE;

        float D = frictionCoefficient * normalForce;

        float BAlpha = B * slipAngle;

        return D * Mathf.Sin(C * Mathf.Atan(BAlpha - E * (BAlpha - Mathf.Atan(BAlpha))));
    }
    private void UpdateWheelVisuals(Transform carBodyTransform, float dt)
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

        float angle = WheelAngularVelocity * Mathf.Rad2Deg * dt;
        wheelTransform.Rotate(Vector3.right, angle, Space.Self);

        Quaternion steeringRotation = Quaternion.Euler(0f, steeringAngle, 0f);
        //wheelTransform.localRotation = steeringRotation;
    }

    public float GetSuspensionCompressionAmount() => Mathf.Max(0f, currentCompression);
    public float GetRestLength() => suspension_RestLength;
    public float GetCurrentCompression() => Mathf.Max(0f, currentCompression);
    public float GetWheelRadius() => wheelRadius;



    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (!ShowRayCastGizmos)
            return;

        float rayLength = suspension_RestLength + suspension_maxExtension + wheelRadius + 0.01f;

        Vector3 origin = transform.position;
        Vector3 direction = -transform.up;
        Vector3 end = origin + direction * rayLength;

        // SphereCast path
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, end);

        // Start sphere
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin, wheelRadius);

        // End sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(end, wheelRadius);

        if (Application.isPlaying && Wheel_isOnGround)
        {
            // Contact point
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitInfo.point, 0.03f);

            // Contact normal
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal * 0.3f);

            // Reconstructed wheel center
            Vector3 wheelCenter = hitInfo.point + hitInfo.normal * wheelRadius;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wheelCenter, wheelRadius);

            // Hub -> wheel center
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, wheelCenter);
        }

        // ===== Slip Angle Visualization =====

        Vector3 gizmoOrigin = hitInfo.point + Vector3.up * 0.02f;

        // Wheel heading (Green)
        Gizmos.color = Color.green;
        Gizmos.DrawRay(
            gizmoOrigin,
            wheelTransform.forward * 0.8f
        );

        // Actual contact patch velocity (Blue)
        Vector3 r = transform.position - transform.root.position;

        CarControllerV2 car = transform.GetComponentInParent<CarControllerV2>();

        if (car != null)
        {
            Vector3 contactPatchVelocity =
                car.carBodyPhysics.GetVelocity() +
                Vector3.Cross(
                    car.carBodyPhysics.GetAngularVelocity(),
                    r
                );

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(
                gizmoOrigin,
                contactPatchVelocity.normalized * 0.8f
            );
        }

        // Lateral tire force (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            gizmoOrigin,
            lateralForce.normalized * 0.8f
        );

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            gizmoOrigin + Vector3.up * 0.1f,
            $"{Mathf.Rad2Deg * slipAngleDebug:F1}°"
        );
#endif
    }

    public void SetDriveTorque(float torque)
    {
        driveTorque = torque;
    }
}
