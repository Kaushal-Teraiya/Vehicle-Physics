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
    //[SerializeField] private float minHeightAboveGround = 0.5f; // adjust based on your chassis height
    private CarBodyCollider bodyCollider;


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
        bodyCollider = new CarBodyCollider(Vector3.zero, carData.carRadius, carData.carHeight);
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


    }
    // void CheckBodyCollisions()
    // {
    //     bodyCollider.GetCapsulePoints(carBodyTransform, out Vector3 p1, out Vector3 p2);

    //     // Raycast sphere along capsule axis
    //     RaycastHit[] hits = Physics.SphereCastAll(
    //         p1,
    //         bodyCollider.radius,
    //         (p2 - p1).normalized,
    //         Vector3.Distance(p2, p1),
    //         layerMask: ~LayerMask.GetMask("Vehicle")  // ignore vehicle layer
    //     );

    //     foreach (var hit in hits)
    //     {
    //         HandleCollision(hit);
    //     }
    // }
    // void HandleCollision(RaycastHit hit)
    // {
    //     Vector3 normal = hit.normal;
    //     float penetration = bodyCollider.radius - Vector3.Distance(carBodyTransform.position, hit.point);

    //     if (penetration > 0.001f)
    //     {
    //         // Separate
    //         carBodyTransform.position += normal * (penetration + 0.001f);

    //         // Decompose velocity
    //         Vector3 vel = carBodyPhysics.GetVelocity();

    //         Vector3 tangentialVel = Vector3.ProjectOnPlane(vel, normal);  // slide along surface
    //         Vector3 normalVel = vel - tangentialVel;  // perpendicular component

    //         // Apply restitution only to normal component
    //         float restitution = 0.3f;
    //         Vector3 correctedVel = tangentialVel - normalVel * restitution;

    //         carBodyPhysics.ResetVelocity();
    //         carBodyPhysics.AddImpulse(correctedVel * carData.carBody_Mass);
    //     }
    // }

    private float carBodyPhysicsMass() => carData.carBody_Mass;
}
