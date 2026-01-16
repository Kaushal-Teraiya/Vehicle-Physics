using UnityEngine;

public class CarSuspension : MonoBehaviour
{
    public Transform carBody;
    public Vector3 bodyRestPos;
    public CarSettings settings;

    public void ApplySuspension(Transform rayOrigin, Transform wheelMesh)
    {
        if (!carBody || !settings)
            return;

        RaycastHit hit;
        Vector3 origin = rayOrigin.position + Vector3.up * settings.YoriginOffset;

        if (
            Physics.Raycast(
                origin,
                Vector3.down,
                out hit,
                settings.suspensionRestLength + settings.suspensionRange
            )
        )
        {
            float compression = settings.suspensionRestLength - (hit.distance - 0.2f);
            compression = Mathf.Clamp(
                compression,
                -settings.suspensionRange,
                settings.suspensionRange
            );

            Vector3 targetPos =
                bodyRestPos + new Vector3(0, compression * settings.compressionMultiplier, 0);
            carBody.localPosition = Vector3.Lerp(
                carBody.localPosition,
                targetPos,
                Time.deltaTime * settings.suspensionDamping
            );

            if (wheelMesh)
            {
                Vector3 wheelLocal = wheelMesh.parent.InverseTransformPoint(hit.point);
                float targetY = wheelLocal.y + settings.wheelRadius;
                Vector3 currentPos = wheelMesh.localPosition;
                currentPos.y = Mathf.Lerp(
                    currentPos.y,
                    targetY,
                    Time.deltaTime * settings.suspensionDamping
                );
                wheelMesh.localPosition = currentPos;
            }
        }
        else
        {
            carBody.localPosition = Vector3.Lerp(
                carBody.localPosition,
                bodyRestPos,
                Time.deltaTime * settings.suspensionDamping
            );
        }
    }
}
