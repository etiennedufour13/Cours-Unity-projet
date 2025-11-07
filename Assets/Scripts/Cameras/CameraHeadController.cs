using UnityEngine;
using UnityEngine.InputSystem;

public class CameraHeadController : MonoBehaviour
{
    [Header("Références")]
    public Transform playerBody;
    public Transform headTransform;
    public Transform cameraTransform;
    public PlayerInput playerInput;

    [Header("Paramètres regard")]
    public float lookSensitivity = 0.15f;
    public bool invertY = false;
    public bool lockCursor = true;

    public float minPitch = -20f;
    public float maxPitch = 45f;

    public float headYawLimit = 70f;
    public float headPitchLimit = 25f;
    public float headSmoothTime = 0.12f;

    public float baseYawOffset = 0f;

    [Header("Paramètre Camera")]
    public Vector3 cameraOffset = new Vector3(0f, 0.6f, -3f);
    public float positionSmoothTime = 0.08f; 
    public float rotationSmoothTime = 0.06f; 
    public float headLookHeight = 0.6f; 


    private InputAction lookAction;
    private Vector3 cameraVelocity = Vector3.zero;
    private float cameraRotVel;
    private float yaw;
    private float pitch;
    private Quaternion headInitialLocalRot;
    private float headYawCurrent;
    private float headPitchCurrent;
    private float headYawVel;
    private float headPitchVel;

    void Awake()
    {
        headInitialLocalRot = headTransform != null ? headTransform.localRotation : Quaternion.identity;

        if (GameManager.Instance.currentState != GameState.Gameplay) return;

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
        if (GameManager.Instance.currentState != GameState.Gameplay) return;

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
