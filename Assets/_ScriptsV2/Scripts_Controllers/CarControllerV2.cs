using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CarControllerV2 : MonoBehaviour
{
    [Header("Data References")]
    public CarData carData;

    [Header("Wheel Controllers")]
    [SerializeField] public List<WheelController> wheelControllers_Scripts = new List<WheelController>();

    [SerializeField] private InputActionReference driveAction;

    private Vector2 driveInput;
    [SerializeField] private Transform carBodyTransform;

    public CarBodyPhysics carBodyPhysics { get; private set; }
    private const float gravity = -9.81f;
    public CarBodyPhysics Physics => carBodyPhysics;
    [SerializeField] private float centerOfMassHeight = 0.5f;
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, 0.35f, 0f);
    private void OnEnable()
    {
        driveAction.action.Enable();
    }

    private void OnDisable()
    {
        driveAction.action.Disable();
    }


    void Start()
    {
        if (wheelControllers_Scripts.Count == 0)
            wheelControllers_Scripts = new List<WheelController>(
                GetComponentsInChildren<WheelController>()
            );

        float totalWheelMass = 0f;
        foreach (var wheel in wheelControllers_Scripts)
        {
            var wheelData = carData.ScriptableObjectOf_WheelData[
                wheelControllers_Scripts.IndexOf(wheel)
            ];
            if (wheelData != null)
            {
                wheel.Initialize(wheelData);
                totalWheelMass += wheelData.MassOf_Wheel;
            }
        }

        float totalMass = carData.carBody_Mass + totalWheelMass;
        carBodyPhysics = new CarBodyPhysics(totalMass);
        // optional: small upward impulse at spawn
        carBodyPhysics.AddImpulse(Vector3.up * 0.5f * totalMass);
    }

    void FixedUpdate()
    {

        driveInput = driveAction.action.ReadValue<Vector2>();
        var chassisVelocity = carBodyPhysics.GetVelocity();
        var chassisAngularVelocity = carBodyPhysics.GetAngularVelocity();
        float throttle = driveInput.y;
        float steeringInput = driveInput.x;
        float dt = Time.fixedDeltaTime;
        Vector3 comWorldPos = carBodyTransform.position + carBodyTransform.TransformDirection(centerOfMassOffset);
        //Debug.Log($"Throttle: {throttle}  Steering: {steering}");
        Vector3 totalForce = Vector3.zero;
        Vector3 totalTorque = Vector3.zero;
        // float totalFriction = 0f;
        // int wheelsOnGround = 0;

        totalForce += Vector3.up * gravity * carBodyPhysicsMass();

        // Suspension forces from each wheel
        foreach (var wheel in wheelControllers_Scripts)
        {
            float steeringAngle = steeringInput * carData.maxSteeringAngle;

            if (wheel.wheelData.canSteer)
            {
                wheel.SteeringAngle(steeringAngle);
            }
            else
            {
                wheel.SteeringAngle(0f);
            }

            float driveTorque = throttle * 1000f;
            wheel.SetDriveTorque(driveTorque);

            wheel.SimulateWheel(dt, carBodyTransform, chassisVelocity, chassisAngularVelocity, comWorldPos);

            totalForce += wheel.wheelForce;
            // Suspension torque


            Vector3 r = wheel.contactPointOf_WheelOnGround - comWorldPos;
            Vector3 localR = carBodyTransform.InverseTransformDirection(r);

            Debug.Log(
                $"Wheel R Local: X:{localR.x:F2} Y:{localR.y:F2} Z:{localR.z:F2}"
            );

            Vector3 wheelTorque = Vector3.Cross(r, wheel.wheelForce);

            totalTorque += wheelTorque;

        }

        Vector3 rollAxis = carBodyTransform.forward;

        float rollAngularVelocity = Vector3.Dot(chassisAngularVelocity, rollAxis);

        float rollDampingTorque = -rollAngularVelocity * 5000f;

        totalTorque += rollAxis * rollDampingTorque;
        Vector3 localTorque = carBodyTransform.InverseTransformDirection(totalTorque);
        Debug.Log(
          $"ROLL TORQUE: {localTorque.z:F0} | " +
          $"ANGULAR Z: {chassisAngularVelocity.z:F3} | " +
          $"FL Fy: {wheelControllers_Scripts[0].LateralForce.magnitude:F0} | " +
          $"FR Fy: {wheelControllers_Scripts[1].LateralForce.magnitude:F0} | " +
          $"RL Fy: {wheelControllers_Scripts[2].LateralForce.magnitude:F0} | " +
          $"RR Fy: {wheelControllers_Scripts[3].LateralForce.magnitude:F0}"
      );
        Debug.Log(
            $"TOTAL TORQUE | Local X:{localTorque.x:F0} " +
            $"Y:{localTorque.y:F0} " +
            $"Z:{localTorque.z:F0}"
        );
        //================= Friction ==================//
        // float averageFriction = wheelsOnGround > 0 ? totalFriction / wheelsOnGround : 0f;
        // carBodyPhysics.ApplyFriction(averageFriction);

        // bool anyWheelOnGround = wheelsOnGround > 0;

        // if (anyWheelOnGround)
        // {
        //     Vector3 angularFriction = new Vector3(0.05f, 0.1f, 0.05f) * averageFriction;
        //     carBodyPhysics.ApplyAngularFriction(angularFriction);
        // }

        //========== linear Motion===========//
        //Vector3 comWorldPos = carBodyTransform.position + carBodyTransform.TransformDirection(centerOfMassOffset)
        totalForce += -carBodyPhysics.GetVelocity() * 0.8f;
        Vector3 displacement = carBodyPhysics.IntegrateLinear(dt, totalForce);
        carBodyTransform.position += displacement;

        //======= Rotational Motion===========//
        Quaternion newRotation = carBodyPhysics.IntegrateRotation(
            dt,
            carBodyTransform.rotation,
            totalTorque
        );
        carBodyTransform.rotation = newRotation;
        // ===================== COLLISION WITH GROUND BOX ===================== //

        //EnforceWheelConstraints();
    }

    void EnforceWheelConstraints()
    {
        foreach (var wheel in wheelControllers_Scripts)
        {
            if (!wheel.Wheel_isOnGround) continue;

            float minSuspensionLength = wheel.GetWheelRadius();
            Vector3 contactPoint = wheel.contactPointOf_WheelOnGround;
            Vector3 mountPos = wheel.transform.position;
            float currentLength = Vector3.Distance(mountPos, contactPoint);

            float penetration = minSuspensionLength - currentLength;

            if (penetration > 0f) // wheel is below ground
            {
                // Apply upward corrective force proportional to penetration depth
                float correctionForce = penetration * 50000f; // tune this stiffness value
                carBodyPhysics.AddImpulse(Vector3.up * correctionForce * Time.fixedDeltaTime);
            }
        }
    }

    private float carBodyPhysicsMass() => carData.carBody_Mass;

    private void OnDrawGizmos()
    {
        if (carBodyTransform == null) return;

        Vector3 comWorldPos =
            carBodyTransform.position +
            carBodyTransform.TransformDirection(centerOfMassOffset);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(comWorldPos, 0.08f);
    }
}
