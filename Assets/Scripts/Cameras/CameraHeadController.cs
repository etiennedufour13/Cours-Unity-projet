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

        if (cameraTransform != null && playerBody != null)
        {
            Vector3 forwardFlat = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            yaw = Quaternion.LookRotation(forwardFlat, Vector3.up).eulerAngles.y;
            pitch = -Vector3.SignedAngle(Vector3.up, cameraTransform.forward, cameraTransform.right);
        }
        else
        {
            yaw = transform.eulerAngles.y;
            pitch = 0f;
        }

        headYawCurrent = 0f;
        headPitchCurrent = 0f;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Start()
    {
        RecenterYawToBody();
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

        Vector2 rawDelta = Vector2.zero;
        if (lookAction != null)
        {
            rawDelta = lookAction.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            rawDelta = Mouse.current.delta.ReadValue();
        }

        float sx = rawDelta.x * lookSensitivity;
        float sy = rawDelta.y * lookSensitivity * (invertY ? -1f : 1f);

        yaw += sx;
        pitch -= sy;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    void LateUpdate()
    {
        if (playerBody == null || headTransform == null || cameraTransform == null)
            return;

        Quaternion targetCamRot = Quaternion.Euler(pitch, yaw + baseYawOffset, 0f);

        Vector3 desiredPos = headTransform.position + targetCamRot * cameraOffset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref cameraVelocity, positionSmoothTime);

        Vector3 lookAtTarget = headTransform.position + Vector3.up * headLookHeight;
        Quaternion desiredLookRot = Quaternion.LookRotation(lookAtTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredLookRot, Time.deltaTime / Mathf.Max(0.0001f, rotationSmoothTime));

        if (cameraTransform != transform)
        {
            cameraTransform.position = transform.position;
            cameraTransform.rotation = transform.rotation;
        }

        float bodyYaw = playerBody.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(bodyYaw, yaw);
        float clampedHeadYaw = Mathf.Clamp(yawDelta, -headYawLimit, headYawLimit);

        float headPitchTarget = Mathf.Clamp(pitch, -headPitchLimit, headPitchLimit);

        headYawCurrent = Mathf.SmoothDampAngle(headYawCurrent, clampedHeadYaw, ref headYawVel, headSmoothTime);
        headPitchCurrent = Mathf.SmoothDamp(headPitchCurrent, headPitchTarget, ref headPitchVel, headSmoothTime);

        Quaternion headOffset = Quaternion.Euler(headPitchCurrent, headYawCurrent, 0f);
        headTransform.localRotation = headOffset * headInitialLocalRot;
    }


    public void RecenterYawToBody(float blendTime = 0.15f)
    {
        if (playerBody == null) return;
        float targetYaw = playerBody.eulerAngles.y;
        yaw = targetYaw;
    }
}
