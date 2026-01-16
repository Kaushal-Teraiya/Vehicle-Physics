using UnityEngine;
using UnityEngine.InputSystem;

public class CarInput : MonoBehaviour
{
    public InputActionReference move;
    public Vector2 InputVector { get; private set; }

    void OnEnable() => move.action.Enable();

    void OnDisable() => move.action.Disable();

    void Update() => InputVector = move.action.ReadValue<Vector2>();
}
