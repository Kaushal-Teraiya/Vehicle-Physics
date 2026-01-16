using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class WheelControllerAssigner : MonoBehaviour
{
    public CarControllerV2 carControllerV2;

#if UNITY_EDITOR
    void OnValidate() => AssignWheelControllers();
#endif

    void Start() => AssignWheelControllers();

    void AssignWheelControllers()
    {
        List<WheelController> wheelControllersList_Cache = new List<WheelController>();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name.ToLower().Contains("wheel"))
            {
                WheelController wheelController = t.GetComponent<WheelController>();
                if (wheelController == null)
                {
                    wheelController = t.gameObject.AddComponent<WheelController>();
                    Debug.Log($"Added missing WheelController to {t.name}");
                }
                wheelControllersList_Cache.Add(wheelController); //if missing or not this line adds the wheelcontroller in the list controllers
            }
        }

        int count = Mathf.Min(
            carControllerV2.carData.ScriptableObjectOf_WheelData.Length,
            wheelControllersList_Cache.Count
        );
        carControllerV2.wheelControllers_Scripts.Clear();

        for (int i = 0; i < count; i++)
        {
            carControllerV2.wheelControllers_Scripts.Add(wheelControllersList_Cache[i]); // this line repopulates the wheelcontroller list on the car controller when there is an accidetal removal of wheel controller script
            // Initialize with CarData
            if (carControllerV2.carData.ScriptableObjectOf_WheelData[i] != null)
                wheelControllersList_Cache[i]
                    .Initialize(carControllerV2.carData.ScriptableObjectOf_WheelData[i]);
            else
                Debug.LogWarning($"WheelData not assigned for wheel index {i}");
        }

        // Debug.Log("Wheel assignment complete!");
    }
}
