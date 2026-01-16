using UnityEngine;

public class CarAudio : MonoBehaviour
{
    public CarSettings settings;
    public AudioSource engineSource;
    public AudioSource skidSource;
    public AudioSource brakeSource;

    [Header("Engine Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2f;
    public float engineIdleVolume = 0.4f;
    public float engineMaxVolume = 1f;

    [Header("Brake Settings")]
    public float brakeVolume = 0.8f;

    float brakeTarget;

    public void UpdateAudio(float currentSpeed, float inputY, float steerInput)
    {
        if (engineSource)
        {
            float speedPercent = Mathf.Abs(currentSpeed) / settings.maxSpeed;
            engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
            engineSource.volume = Mathf.Lerp(engineIdleVolume, engineMaxVolume, speedPercent);
        }

        if (brakeSource)
        {
            bool braking = inputY < 0 && Mathf.Abs(currentSpeed) > 5f;
            brakeTarget = braking ? brakeVolume : 0f;
            brakeSource.volume = Mathf.MoveTowards(
                brakeSource.volume,
                brakeTarget,
                Time.deltaTime * 3f
            );
            if (!brakeSource.isPlaying && brakeSource.volume > 0.05f)
                brakeSource.Play();
            if (brakeSource.isPlaying && brakeSource.volume < 0.05f)
                brakeSource.Stop();
        }
    }
}
