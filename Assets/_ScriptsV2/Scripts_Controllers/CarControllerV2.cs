using System.Collections.Generic;
using UnityEngine;

public class CarControllerV2 : MonoBehaviour
{
    [Header("Data References")]
    public CarData carData;

    [Header("Wheel Controllers")]
    [SerializeField]
    public List<WheelController> wheelControllers_Scripts = new List<WheelController>();

    [SerializeField]
    private Transform carBodyTransform;

    private CarBodyPhysics carBodyPhysics;
    private const float gravity = -9.81f;
    public VehicleCollider vehicleCollider;
    public StaticBoxCollider groundCollider;

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
        float dt = Time.fixedDeltaTime;

        Vector3 totalForce = Vector3.zero;
        Vector3 totalTorque = Vector3.zero;
        float totalFriction = 0f;
        int wheelsOnGround = 0;

        totalForce += Vector3.up * gravity * carBodyPhysicsMass();

        // Suspension forces from each wheel
        foreach (var wheel in wheelControllers_Scripts)
        {
            wheel.SimulateWheel(dt, carBodyTransform);
            totalForce += wheel.wheelForce;

            Vector3 r = wheel.transform.position - carBodyTransform.position;
            totalTorque += Vector3.Cross(r, wheel.wheelForce);

            float wheel_Friction = wheel.GetFrictionCoefficient();

            if (wheel_Friction > 0f)
            {
                totalFriction += wheel_Friction;
                wheelsOnGround++;
            }
        }

        //================= Friction ==================//
        float averageFriction = wheelsOnGround > 0 ? totalFriction / wheelsOnGround : 0f;
        carBodyPhysics.ApplyFriction(averageFriction);

        bool anyWheelOnGround = wheelsOnGround > 0;

        if (anyWheelOnGround)
        {
            Vector3 angularFriction = new Vector3(0.05f, 0.1f, 0.05f) * averageFriction;
            carBodyPhysics.ApplyAngularFriction(angularFriction);
        }

        //========== linear Motion===========//
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
        if (groundCollider != null && vehicleCollider != null)
        {
            if (
                CollisionMath.OBBvsOBB(
                    vehicleCollider.OBB,
                    groundCollider.OBB,
                    out Vector3 normal,
                    out float penetration
                )
            )
            {
                // Move car out of ground
                Vector3 correction = normal * penetration;
                carBodyTransform.position += correction;

                // Stop downward velocity
                Vector3 vel = carBodyPhysics.GetVelocity();
                float into = Vector3.Dot(vel, -normal);

                if (into > 0)
                {
                    vel += normal * into;
                    carBodyPhysics.ResetVelocity();
                    carBodyPhysics.AddImpulse(vel * carBodyPhysicsMass());
                }
            }
        }
    }

    private float carBodyPhysicsMass() => carData.carBody_Mass;
}
