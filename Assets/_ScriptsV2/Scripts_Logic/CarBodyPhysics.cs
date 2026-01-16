using UnityEngine;

[System.Serializable]
public class CarBodyPhysics
{
    private float mass;
    private Vector3 velocity;
    private const float velocityDamping = 0.985f; // very mild damping

    private Vector3 angularVelocity;
    private Vector3 inertia;
    private Vector3 inverseInertia;
    private const float angularDamping = 0.995f;

    public CarBodyPhysics(float mass)
    {
        this.mass = mass;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;

        inertia = new Vector3(300f, 1200f, 500f); // how hard the object is to rotate in x , y , z axes
        inverseInertia = new Vector3(1f / inertia.x, 1f / inertia.y, 1f / inertia.z); // for ease   torque formula is τ = Iα and α = τ / I   angular acceleration
    }

    // Integrate linear motion
    public Vector3 IntegrateLinear(float dt, Vector3 totalForce)
    {
        Vector3 acceleration = totalForce / mass;
        velocity += acceleration * dt;
        velocity *= velocityDamping;
        return velocity * dt;
    }

    public Quaternion IntegrateRotation(
        float dt_fixedDeltaTime,
        Quaternion currentRotation,
        Vector3 totalTorque
    )
    {
        Vector3 Torque_LocalSpace = Quaternion.Inverse(currentRotation) * totalTorque; // Inverse of rotation axes from world to local so the torque is applied on the local car axes

        Vector3 angularAcceleration_Local = new Vector3(
            Torque_LocalSpace.x * inverseInertia.x,
            Torque_LocalSpace.y * inverseInertia.y,
            Torque_LocalSpace.z * inverseInertia.z
        );

        Vector3 angularAcceleration_World = currentRotation * angularAcceleration_Local;

        // ω
        angularVelocity += angularAcceleration_World * dt_fixedDeltaTime;
        angularVelocity *= angularDamping;

        Quaternion deltaRotation = Quaternion.Euler(
            angularVelocity * Mathf.Rad2Deg * dt_fixedDeltaTime
        );
        return deltaRotation * currentRotation;
    }

    public void ApplyFriction(float frictionCoefficient)
    {
        if (frictionCoefficient <= 0f)
        {
            return;
        }

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 frictionImpulse = -horizontalVelocity * frictionCoefficient;
        velocity.x += frictionImpulse.x;
        velocity.z += frictionImpulse.z;

        if (horizontalVelocity.magnitude < 0.05f)
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }

    public void ApplyAngularFriction(Vector3 angularFriction)
    {
        angularVelocity = new Vector3(
            angularVelocity.x * (1f - angularFriction.x), // 1 - angularFriction is the damper like factor that reduces velocity this done every frame
            angularVelocity.y * (1f - angularFriction.y),
            angularVelocity.z * (1f - angularFriction.z)
        );

        if (angularVelocity.magnitude < 0.05f)
        {
            angularVelocity = Vector3.zero;
        }
    }

    public Vector3 GetVelocity() => velocity;

    public void AddImpulse(Vector3 impulse) => velocity += impulse / mass;

    public void ResetVelocity() => velocity = Vector3.zero;

    public void GetAngularVelocity(Vector3 impulse) => velocity += impulse / mass;

    public void Reset()
    {
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }
}
