using UnityEngine;

public class CarController : MonoBehaviour
{
    public CarSettings settings;
    public CarInput input;
    public CarSuspension suspension;
    public CarCameraShake cameraShake;
    public CarAudio carAudio;

    public Transform frontLeft,
        frontRight,
        backLeft,
        backRight;
    public Transform wheelFLMesh,
        wheelFRMesh,
        wheelBLMesh,
        wheelBRMesh;
    public Transform wheelFLSteer,
        wheelFRSteer,
        steerWheel;

    CharacterController controller;
    Vector3 moveDir;
    float currentSpeed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        suspension.bodyRestPos = suspension.carBody.localPosition;
    }

    void Update()
    {
        Vector2 _Input = input.InputVector;

        if (_Input.y > 0)
            currentSpeed += settings.acceleration * Time.deltaTime;
        else if (_Input.y < 0)
            currentSpeed -= settings.deceleration * Time.deltaTime;
        else
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0,
                settings.decelerationToRest * Time.deltaTime
            );

        currentSpeed = Mathf.Clamp(currentSpeed, -settings.maxSpeed / 2f, settings.maxSpeed);

        if (Mathf.Abs(currentSpeed) > 0.1f)
            transform.Rotate(0, _Input.x * settings.turnSpeed * Time.deltaTime, 0);

        float targetSteer = _Input.x * settings.maxSteerAngle;
        float targetSteerz = -_Input.x * settings.maxSteerAngle;

        Vector3 rotL = wheelFLSteer.localEulerAngles;
        if (rotL.y > 180)
            rotL.y -= 360;
        rotL.y = Mathf.Lerp(rotL.y, targetSteer, Time.deltaTime * 5f);
        wheelFLSteer.localEulerAngles = rotL;

        Vector3 rotR = wheelFRSteer.localEulerAngles;
        if (rotR.y > 180)
            rotR.y -= 360;
        rotR.y = Mathf.Lerp(rotR.y, targetSteer, Time.deltaTime * 5f);
        wheelFRSteer.localEulerAngles = rotR;

        Vector3 rotSteer = steerWheel.localEulerAngles;
        if (rotSteer.z > 180)
            rotSteer.z -= 360;
        rotSteer.z = Mathf.Lerp(rotSteer.z, targetSteerz, Time.deltaTime * 5f);
        steerWheel.localEulerAngles = rotSteer;

        moveDir = transform.forward * currentSpeed;

        Transform[] wheels = { frontLeft, frontRight, backLeft, backRight };
        Vector3 avgNormal = Vector3.zero;
        int hitCount = 0;

        foreach (Transform wheel in wheels)
        {
            RaycastHit hit;
            if (
                Physics.Raycast(
                    wheel.position + Vector3.up * 0.2f,
                    transform.forward,
                    out hit,
                    Mathf.Abs(currentSpeed * Time.deltaTime) + settings.collisionRayLength
                )
            )
            {
                currentSpeed *= 0.7f;
                moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal);
            }

            if (Physics.Raycast(wheel.position + Vector3.up * 0.2f, Vector3.down, out hit, 1f))
            {
                avgNormal += hit.normal;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            avgNormal /= hitCount;
            Quaternion targetRot =
                Quaternion.FromToRotation(transform.up, avgNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 5f
            );
        }

        foreach (Transform wheel in wheels)
        {
            RaycastHit hit;
            if (
                Physics.Raycast(
                    wheel.position,
                    Vector3.down,
                    out hit,
                    settings.suspensionRestLength + 0.5f
                )
            )
            {
                float compression = settings.suspensionRestLength - hit.distance;
                moveDir += Vector3.up * compression * settings.suspensionStrength * Time.deltaTime;
            }
        }

        moveDir += Vector3.down * 9.8f * Time.deltaTime;
        controller.Move(moveDir * Time.deltaTime);

        float spin = currentSpeed * settings.wheelRotationMultiplier * Time.deltaTime;
        wheelFLMesh.Rotate(spin, 0, 0);
        wheelFRMesh.Rotate(spin, 0, 0);
        wheelBLMesh.Rotate(spin, 0, 0);
        wheelBRMesh.Rotate(spin, 0, 0);

        suspension.ApplySuspension(frontLeft, wheelFLSteer);
        suspension.ApplySuspension(frontRight, wheelFRSteer);
        suspension.ApplySuspension(backLeft, wheelBLMesh);
        suspension.ApplySuspension(backRight, wheelBRMesh);

        cameraShake.ApplyShake(currentSpeed);
        carAudio.UpdateAudio(currentSpeed, _Input.y, _Input.x);
    }
}
