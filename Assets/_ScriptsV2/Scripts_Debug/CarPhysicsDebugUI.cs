using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CarPhysicsDebugUI : MonoBehaviour
{
    [SerializeField] private CarControllerV2 carController;
    [SerializeField] private bool startVisible = true;

    private Canvas canvas;
    private GameObject dashboard;
    private Text chassisText;
    private readonly Text[] wheelTexts = new Text[4];

    private InputAction toggleAction;

    private FieldInfo carBodyPhysicsField;
    private FieldInfo angularVelocityField;
    private FieldInfo massField;

    private readonly Dictionary<string, FieldInfo> wheelFields = new();

    private void Awake()
    {
        if (carController == null)
            carController = GetComponentInParent<CarControllerV2>();

        if (carController == null)
        {
            Debug.LogError("[PhysicsUI] CarControllerV2 not found.");
            enabled = false;
            return;
        }

        toggleAction = new InputAction(
            "Toggle Physics UI",
            InputActionType.Button,
            "<Keyboard>/f3"
        );

        toggleAction.Enable();

        CachePhysicsFields();
        BuildUI();
        dashboard.SetActive(startVisible);
    }

    private void Update()
    {
        if (toggleAction != null && toggleAction.WasPressedThisFrame())
            ToggleDashboard();

        if (dashboard != null && dashboard.activeSelf)
            UpdateUI();
    }

    private void ToggleDashboard()
    {
        if (dashboard != null)
            dashboard.SetActive(!dashboard.activeSelf);
    }

    public void ToggleFromButton()
    {
        ToggleDashboard();
    }

    // ============================================================
    // UI
    // ============================================================

    private void BuildUI()
    {
        GameObject canvasObject = new("Car Physics Debug Canvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        dashboard = new GameObject("Dashboard");
        dashboard.transform.SetParent(canvasObject.transform, false);

        RectTransform root = dashboard.AddComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(1600f, 820f);

        CreateHeader(root);
        CreateChassis(root);
        CreateWheels(root);
        CreateFooter(root);
    }

    private void CreateHeader(RectTransform root)
    {
        GameObject panel = CreatePanel(
            "Header",
            root,
            .015f,
            .93f,
            .985f,
            .985f
        );

        CreateText(
            panel.transform,
            "CAR PHYSICS TELEMETRY",
            22,
            TextAnchor.MiddleLeft,
            .02f,
            .1f,
            .7f,
            .9f
        );

        CreateButton(
            panel.transform,
            "F3  TOGGLE",
            .78f,
            .12f,
            .98f,
            .88f
        );
    }

    private void CreateChassis(RectTransform root)
    {
        GameObject panel = CreatePanel(
            "Chassis",
            root,
            .015f,
            .72f,
            .985f,
            .92f
        );

        CreateText(
            panel.transform,
            "CHASSIS",
            15,
            TextAnchor.MiddleLeft,
            .03f,
            .88f,
            .97f,
            .99f
        );

        chassisText = CreateText(
            panel.transform,
            "",
            13,
            TextAnchor.UpperLeft,
            .025f,
            .05f,
            .975f,
            .86f
        );
    }

    private void CreateWheels(RectTransform root)
    {
        string[] names =
        {
            "FRONT LEFT",
            "FRONT RIGHT",
            "REAR LEFT",
            "REAR RIGHT"
        };

        float gap = .008f;
        float width = (.97f - gap * 3f) / 4f;

        for (int i = 0; i < 4; i++)
        {
            float x = .015f + i * (width + gap);

            GameObject panel = CreatePanel(
                names[i],
                root,
                x,
                .16f,
                x + width,
                .70f
            );

            CreateText(
                panel.transform,
                names[i],
                15,
                TextAnchor.MiddleLeft,
                .05f,
                .90f,
                .95f,
                .99f
            );

            wheelTexts[i] = CreateText(
                panel.transform,
                "",
                12,
                TextAnchor.UpperLeft,
                .05f,
                .025f,
                .95f,
                .88f
            );
        }
    }

    private void CreateFooter(RectTransform root)
    {
        GameObject panel = CreatePanel(
            "Footer",
            root,
            .015f,
            .03f,
            .985f,
            .13f
        );

        CreateText(
            panel.transform,
            "LIVE PHYSICS TELEMETRY",
            12,
            TextAnchor.MiddleLeft,
            .02f,
            .1f,
            .98f,
            .9f
        );
    }

    private GameObject CreatePanel(
        string name,
        Transform parent,
        float xMin,
        float yMin,
        float xMax,
        float yMax)
    {
        GameObject panel = new(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(.015f, .025f, .03f, .95f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(.12f, .25f, .32f, 1f);
        outline.effectDistance = new Vector2(1, -1);

        return panel;
    }

    private Text CreateText(
        Transform parent,
        string value,
        int size,
        TextAnchor alignment,
        float xMin,
        float yMin,
        float xMax,
        float yMax)
    {
        GameObject obj = new("Text");
        obj.transform.SetParent(parent, false);

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = true;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        return text;
    }

    private void CreateButton(
        Transform parent,
        string label,
        float xMin,
        float yMin,
        float xMax,
        float yMax)
    {
        GameObject buttonObject = new("Toggle Button");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(.04f, .08f, .1f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(ToggleFromButton);

        CreateText(
            buttonObject.transform,
            label,
            12,
            TextAnchor.MiddleCenter,
            0f,
            0f,
            1f,
            1f
        );
    }

    // ============================================================
    // PHYSICS ACCESS
    // ============================================================

    private void CachePhysicsFields()
    {
        carBodyPhysicsField = typeof(CarControllerV2).GetField(
            "carBodyPhysics",
            BindingFlags.Instance |
            BindingFlags.NonPublic
        );

        if (carBodyPhysicsField == null)
            return;

        System.Type type = carBodyPhysicsField.FieldType;

        angularVelocityField = type.GetField(
            "angularVelocity",
            BindingFlags.Instance |
            BindingFlags.NonPublic
        );

        massField = type.GetField(
            "mass",
            BindingFlags.Instance |
            BindingFlags.NonPublic
        );
    }

    private object GetBodyPhysics()
    {
        return carBodyPhysicsField?.GetValue(carController);
    }

    private Vector3 GetVelocity()
    {
        return carController.Physics.GetVelocity();
    }

    private Vector3 GetAngularVelocity()
    {
        return carController.Physics.GetAngularVelocity();
    }

    private float GetMass()
    {
        return carController.Physics.GetMass();
    }

    private object GetWheelField(WheelController wheel, string name)
    {
        if (wheel == null)
            return null;

        System.Type type = wheel.GetType();
        string key = type.Name + name;

        if (!wheelFields.TryGetValue(key, out FieldInfo field))
        {
            field = type.GetField(
                name,
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public
            );

            wheelFields[key] = field;
        }

        return field?.GetValue(wheel);
    }

    private T GetWheelField<T>(
        WheelController wheel,
        string name,
        T fallback = default)
    {
        object value = GetWheelField(wheel, name);
        return value is T result ? result : fallback;
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void UpdateUI()
    {
        UpdateChassis();

        if (carController.wheelControllers_Scripts == null)
            return;

        for (
            int i = 0;
            i < Mathf.Min(4, carController.wheelControllers_Scripts.Count);
            i++
        )
        {
            UpdateWheel(
                i,
                carController.wheelControllers_Scripts[i]
            );
        }
    }

    private void UpdateChassis()
    {
        Vector3 velocity = GetVelocity();
        Vector3 angular = GetAngularVelocity();
        Vector3 euler = carController.transform.eulerAngles;

        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);
        float roll = NormalizeAngle(euler.z);

        Vector3 force =
            Vector3.up * -9.81f * GetMass();

        Vector3 torque = Vector3.zero;

        foreach (WheelController wheel in carController.wheelControllers_Scripts)
        {
            if (wheel == null)
                continue;

            force += wheel.wheelForce;

            Vector3 r =
                wheel.transform.position -
                carController.transform.position;

            torque += Vector3.Cross(
                r,
                wheel.wheelForce
            );
        }

        chassisText.text =
            $"<b>MASS</b>       {GetMass():F1} kg\n" +
            $"<b>VELOCITY</b>   X {velocity.x:F2}  Y {velocity.y:F2}  Z {velocity.z:F2}\n" +
            $"<b>SPEED</b>      {velocity.magnitude:F2} m/s  |  {velocity.magnitude * 3.6f:F2} km/h\n\n" +
            $"<b>ANGULAR ω</b>  X {angular.x:F3}  Y {angular.y:F3}  Z {angular.z:F3}\n" +
            $"<b>ROTATION</b>   P {pitch:F2}°  Y {yaw:F2}°  R {roll:F2}°\n\n" +
            $"<b>FORCE</b>      X {force.x:F0}  Y {force.y:F0}  Z {force.z:F0} N\n" +
            $"<b>TORQUE</b>     X {torque.x:F0}  Y {torque.y:F0}  Z {torque.z:F0} Nm";
    }

    private void UpdateWheel(int index, WheelController wheel)
    {
        if (wheel == null || wheelTexts[index] == null)
            return;

        bool grounded = wheel.Wheel_isOnGround;

        float friction =
            GetWheelField<float>(
                wheel,
                "currentSurfaceFrictionCoefficient"
            );

        float compression = wheel.currentCompression;
        float restLength = wheel.suspension_RestLength;
        float maxCompression = wheel.suspension_maxCompression;
        float radius = wheel.wheelRadius;

        float normalForce =
            wheel.suspensionForce.magnitude;

        float omega = wheel.WheelAngularVelocity;
        float surfaceSpeed = omega * radius;

        Vector3 velocity = GetVelocity();
        Vector3 angular = GetAngularVelocity();

        Vector3 r =
            wheel.transform.position -
            carController.transform.position;

        Vector3 contactVelocity =
            velocity +
            Vector3.Cross(angular, r);
        float steeringAngle = wheel.steeringAngle;

        Quaternion steeringRotation = Quaternion.AngleAxis(
            steeringAngle,
            wheel.transform.up
        );

        Vector3 wheelForward = steeringRotation * wheel.transform.forward;
        Vector3 wheelRight = steeringRotation * wheel.transform.right;

        float forwardVelocity = Vector3.Dot(
            contactVelocity,
            wheelForward
        );

        float lateralVelocity = Vector3.Dot(
            contactVelocity,
            wheelRight
        );
        float referenceSpeed =
            Mathf.Max(
                Mathf.Abs(forwardVelocity),
                0.1f
            );

        float slipRatio =
            Mathf.Clamp(
                (surfaceSpeed - forwardVelocity) /
                referenceSpeed,
                -3f,
                3f
            );

        float slipAngle =
            Mathf.Atan2(
                lateralVelocity,
                Mathf.Abs(forwardVelocity)
            ) * Mathf.Rad2Deg;

        // ========================================================
        // WHEEL FORCES
        // ========================================================

        Vector3 suspensionForce =
            wheel.suspensionForce;

        Vector3 longitudinalForce =
            GetWheelField<Vector3>(
                wheel,
                "LongitudinalForce"
            );

        Vector3 lateralForce =
            GetWheelField<Vector3>(
                wheel,
                "lateralForce"
            );

        // Actual drive torque being sent into WheelController.
        float engineTorque =
            GetWheelField<float>(
                wheel,
                "driveTorque"
            );

        float netTorque =
            GetWheelField<float>(
                wheel,
                "netTorque"
            );

        float reactionTorque =
            engineTorque - netTorque;

        float fx = Vector3.Dot(
      longitudinalForce,
      wheelForward
  );

        float fy = Vector3.Dot(
            lateralForce,
            wheelRight
        );

        float compressionPercent =
            maxCompression > 0f
                ? Mathf.Max(0f, compression) /
                  maxCompression *
                  100f
                : 0f;

        string state = grounded
            ? "<color=#55DD88>● GROUNDED</color>"
            : "<color=#FF5555>○ AIRBORNE</color>";

        // ========================================================
        // TELEMETRY
        // ========================================================

        wheelTexts[index].text =
            $"{state}\n\n" +

            $"<b>μ</b>             {friction:F3}\n\n" +

            $"<b>SUSPENSION</b>\n" +
            $"Compression   {compression:F3} m\n" +
            $"Travel        {compressionPercent:F1} %\n" +
            $"Rest Length   {restLength:F3} m\n" +
            $"Normal Force  {normalForce:F0} N\n\n" +

            $"<b>VELOCITY</b>\n" +
            $"Vehicle       {forwardVelocity:F3} m/s\n" +
            $"Wheel         {surfaceSpeed:F3} m/s\n" +
            $"Lateral       {lateralVelocity:F3} m/s\n\n" +

            $"<b>TIRE</b>\n" +
            $"Slip Ratio    {slipRatio:F4}\n" +
            $"Slip Angle    {slipAngle:F2}°\n\n" +

            $"<b>FORCES</b>\n" +
            $"Fx            {fx:F0} N\n" +
            $"Fy            {fy:F0} N\n" +
            $"Fz            {normalForce:F0} N\n\n" +

            $"<b>WORLD FORCE</b>\n" +
            $"Suspension    {FormatVector(suspensionForce)}\n" +
            $"Longitudinal  {FormatVector(longitudinalForce)}\n" +
            $"Lateral       {FormatVector(lateralForce)}\n\n" +

            $"<b>WHEEL</b>\n" +
            $"ω             {omega:F3} rad/s\n" +
            $"Surface       {surfaceSpeed:F3} m/s\n\n" +

            $"<b>TORQUE</b>\n" +
            $"Engine        {engineTorque:F1} Nm\n" +
            $"Reaction      {reactionTorque:F1} Nm\n" +
            $"Net           {netTorque:F1} Nm";
    }

    private string FormatVector(Vector3 value)
    {
        return $"X {value.x:F0}  Y {value.y:F0}  Z {value.z:F0} N";
    }

    private float NormalizeAngle(float angle)
    {
        return angle > 180f
            ? angle - 360f
            : angle;
    }

    private void OnDestroy()
    {
        if (toggleAction != null)
        {
            toggleAction.Disable();
            toggleAction.Dispose();
        }

        if (canvas != null)
            Destroy(canvas.gameObject);
    }
}