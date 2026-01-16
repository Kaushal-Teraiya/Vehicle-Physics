using UnityEngine;

public class CarCameraShake : MonoBehaviour
{
    public Transform playerCamera;
    public CarSettings settings;
    public Vector3 cameraDefaultPos;
    Vector3 shakeOffset;

    public void ApplyShake(float currentSpeed)
    {
        if (!playerCamera || !settings)
            return;

        float speedIntensity =
            settings.baseShakeIntensity + Mathf.Abs(currentSpeed) * settings.speedShakeMultiplier;
        Vector3 speedShake = Random.insideUnitSphere * speedIntensity;
        shakeOffset = Vector3.Lerp(shakeOffset, speedShake, Time.deltaTime * settings.shakeDamping);
        playerCamera.localPosition = cameraDefaultPos + shakeOffset;
    }
}
