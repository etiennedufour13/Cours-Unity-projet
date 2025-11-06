// CameraHeadController.cs
// Unity 6.2 - New Input System compatible
// Attach this to an independent CameraRig GameObject (not the Camera itself).
// Setup (in-inspector):
// - playerBody: the Transform representing the lower body / root that moves/rotates with the wheels.
// - headTransform: the Transform of the head mesh (child of player).
// - cameraTransform: the actual Camera (child or separate) that will be positioned by this script.
// - PlayerInput (optional) with an action named "Look" (Vector2). If absent, script falls back to Mouse.current.delta.
// Notes:
// - The script keeps the camera independent from body movement and allows the head to turn toward the camera aim.
// - Tune sensitivities, limits and smooth times in the inspector for the desired gamefeel.

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHeadController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform that represents the moving body (wheels).")]
    public Transform playerBody;
    [Tooltip("Transform of the head part (will rotate independently).")]
    public Transform headTransform;
    [Tooltip("The Camera transform (the actual camera).")]
    public Transform cameraTransform;
    [Tooltip("Optional PlayerInput containing a 'Look' action (Vector2). If null, Mouse.current.delta is used.")]
    public PlayerInput playerInput;
    [Tooltip("Name of the look action inside the PlayerInput asset.")]
    public string lookActionName = "Look";

    [Header("Mouse / Look")]
    public float lookSensitivity = 0.15f;      // multiplier applied to raw pointer delta
    public bool invertY = false;
    public bool lockCursor = true;

    [Header("Camera Offset")]
    public Vector3 cameraOffset = new Vector3(0f, 0.6f, -3f); // local offset relative to head (z negative = behind)
    public float positionSmoothTime = 0.08f;   // smaller = snappier
    public float rotationSmoothTime = 0.06f;   // for camera LookAt smoothing
    public float headLookHeight = 0.6f;        // vertical offset the camera looks at on the head

    [Header("Pitch Limits")]
    public float minPitch = -20f; // look down
    public float maxPitch = 45f;  // look up

    [Header("Head constraints")]
    public float headYawLimit = 70f;   // degrees left/right relative to body forward
    public float headPitchLimit = 25f; // degrees up/down relative to neutral head
    public float headSmoothTime = 0.12f;

    [Header("Rotation Offset")]
    public float baseYawOffset = 0f;


    // internals
    private InputAction lookAction;
    private Vector3 cameraVelocity = Vector3.zero;
    private float cameraRotVel;
    private float yaw;   // world yaw that camera is facing
    private float pitch; // camera pitch
    private Quaternion headInitialLocalRot;
    private float headYawCurrent;   // smoothed head yaw offset (deg)
    private float headPitchCurrent; // smoothed head pitch offset (deg)
    private float headYawVel;
    private float headPitchVel;

    void Awake()
    {
        if (playerInput != null)
        {
            if (playerInput.actions != null && playerInput.actions.FindAction(lookActionName) != null)
                lookAction = playerInput.actions[lookActionName];
            else
                lookAction = null;
        }

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (headTransform == null)
            Debug.LogWarning("CameraHeadController: headTransform not assigned.");

        if (playerBody == null)
            Debug.LogWarning("CameraHeadController: playerBody not assigned.");

        headInitialLocalRot = headTransform != null ? headTransform.localRotation : Quaternion.identity;

        // Initialize yaw/pitch from current camera orientation so the first frame doesn't snap
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = cameraTransform != null ? cameraTransform.eulerAngles.x : 0f;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnEnable()
    {
        if (lookAction != null)
            lookAction.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null)
            lookAction.Disable();
    }

    void Update()
    {
        // Read look input (delta)
        Vector2 rawDelta = Vector2.zero;
        if (lookAction != null)
        {
            rawDelta = lookAction.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            rawDelta = Mouse.current.delta.ReadValue();
        }

        // Scale input by sensitivity (we multiply by a small scalar to have fine control)
        float sx = rawDelta.x * lookSensitivity;
        float sy = rawDelta.y * lookSensitivity * (invertY ? -1f : 1f);

        // Integrate into yaw/pitch
        yaw += sx;
        pitch -= sy;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (playerBody == null || headTransform == null || cameraTransform == null)
            return;

        // compute desired camera rotation (world yaw/pitch)
        Quaternion targetCamRot = Quaternion.Euler(pitch, yaw + baseYawOffset, 0f);


        // desired camera world position from head position + rotated offset
        Vector3 desiredPos = headTransform.position + targetCamRot * cameraOffset;

        // smooth position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref cameraVelocity, positionSmoothTime);

        // smooth rotation so camera looks at head with small smoothing
        Vector3 lookAtTarget = headTransform.position + Vector3.up * headLookHeight;
        Quaternion desiredLookRot = Quaternion.LookRotation(lookAtTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredLookRot, Time.deltaTime / Mathf.Max(0.0001f, rotationSmoothTime));

        // place/orient the actual cameraTransform to match this rig (cameraTransform may be child or separate)
        // We keep cameraTransform at the rig position+offset for possible child-camera adjustments (post-processing)
        if (cameraTransform != transform)
        {
            cameraTransform.position = transform.position;
            cameraTransform.rotation = transform.rotation;
        }

        // HEAD ORIENTATION (relative to playerBody)
        // compute angular difference between camera yaw and body yaw (signed)
        float bodyYaw = playerBody.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(bodyYaw, yaw); // camera yaw minus body yaw
        float clampedHeadYaw = Mathf.Clamp(yawDelta, -headYawLimit, headYawLimit);

        // head pitch: derive from camera pitch but limited to headPitchLimit relative to neutral
        float headPitchTarget = Mathf.Clamp(pitch, -headPitchLimit, headPitchLimit);

        // smooth head yaw & pitch
        headYawCurrent = Mathf.SmoothDampAngle(headYawCurrent, clampedHeadYaw, ref headYawVel, headSmoothTime);
        headPitchCurrent = Mathf.SmoothDamp(headPitchCurrent, headPitchTarget, ref headPitchVel, headSmoothTime);

        // apply to head local rotation: we build a local rotation offset from the initial local rotation
        Quaternion headOffset = Quaternion.Euler(headPitchCurrent, headYawCurrent, 0f);
        headTransform.localRotation = headOffset * headInitialLocalRot;
    }

    // Optional helper: public method to recenter camera yaw on body (call when respawning or on specific input)
    public void RecenterYawToBody(float blendTime = 0.15f)
    {
        if (playerBody == null) return;
        float targetYaw = playerBody.eulerAngles.y;
        // set yaw to body yaw immediately, but keep pitch as-is
        yaw = targetYaw;
        // head will smoothly catch up in LateUpdate
    }
}
