using UnityEngine;

[System.Serializable]
public class WheelPhysics
{
    private float wheelInertia;

    private float angularVelocity;     // rad/s
    private float rotationAngle;       // radians

    public WheelPhysics(float wheelMass, float wheelRadius)
    {
        wheelInertia = 0.5f * wheelMass * wheelRadius * wheelRadius;

        angularVelocity = 0f;
        rotationAngle = 0f;
    }


    public void IntegrateRotation(float dt, float appliedTorque)
    {
        float angularAcceleration = appliedTorque / wheelInertia;

        angularVelocity += angularAcceleration * dt;

        rotationAngle += angularVelocity * dt;
    }

    public float GetSurfaceSpeed(float wheelRadius) => angularVelocity * wheelRadius;

    public float GetAngularVelocity() => angularVelocity;

    public float GetRotationAngle() => rotationAngle;
    public float GetWheelInertia() => wheelInertia;

    public void Reset()
    {
        angularVelocity = 0f;
        rotationAngle = 0f;
    }
}