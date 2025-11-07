using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Références Input System")]
    public PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    [Header("Mouvements")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float airControl = 0.6f;
    [SerializeField] private float velocitySmoothing = 0.04f;

    [Header("Saut et Gravité")]
    [SerializeField] private float jumpVelocity = 6.5f;
    [SerializeField] private float fallMultiplier = 2.6f;

    [Header("Glide")]
    [SerializeField] private bool enableGlide = true;
    [SerializeField] private float glideVerticalTarget = -0.3f;
    [SerializeField] private float glideActivationThreshold = -0.15f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    [Header("Wheel Rotation")]
    public Transform[] leftWheels, rightWheels;
    public float wheelRotationSpeed = 360f;

    [Header("Son")]
    public AudioSource moteurSon;
    public float minPitch = 0.9f;
    public float maxPitch = 1.8f;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;
    public float maxSpeedForAudio = 10f;


    private Rigidbody rb;
    private bool isGrounded;
    private bool jumpRequested = false;
    private bool jumpHeld;
    private Vector3 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        jumpAction.started += ctx => jumpHeld = true;
        jumpAction.canceled += ctx => jumpHeld = false;
    }

    private void Update()
    {
        if (GameManager.Instance.currentState != GameState.Gameplay) return;

        float rotation = moveAction.ReadValue<Vector2>().x;
        if (Mathf.Abs(rotation) > 0.1f)
        {
            float turn = rotation * rotationSpeed * Time.deltaTime;
            rb.MoveRotation(Quaternion.Euler(0f, transform.eulerAngles.y + turn, 0f));
        }

        if (jumpAction.triggered)
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.currentState != GameState.Gameplay) return;
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleGlide();
        ApplyFallMultiplier();
    }

    private void HandleMovement()
    {
        float inputForward = moveAction.ReadValue<Vector2>().y;
        float control = isGrounded ? 1f : airControl;

        Vector3 desiredVelocity = transform.forward * (inputForward * moveSpeed);
        Vector3 currentVel = rb.linearVelocity;
        Vector3 targetVel = new Vector3(desiredVelocity.x, currentVel.y, desiredVelocity.z);

        if (velocitySmoothing > 0f)
        {
            Vector3 smoothVel = Vector3.SmoothDamp(currentVel, targetVel, ref currentVelocity, velocitySmoothing);
            rb.linearVelocity = new Vector3(smoothVel.x, currentVel.y, smoothVel.z);
        }
        else
        {
            rb.linearVelocity = targetVel;
        }

        float forwardSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        foreach (Transform wheel in leftWheels)
        {
            if (wheel == null) continue;
            wheel.Rotate(Vector3.down * forwardSpeed * wheelRotationSpeed * Time.deltaTime, Space.Self);
        }
        foreach (Transform wheel in rightWheels)
        {
            if (wheel == null) continue;
            wheel.Rotate(Vector3.up * forwardSpeed * wheelRotationSpeed * Time.deltaTime, Space.Self);
        }

        float normalizedSpeed = Mathf.Clamp01(forwardSpeed / maxSpeedForAudio);

        moteurSon.volume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);
        moteurSon.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);

    }

    private void HandleJump()
    {
        if (jumpRequested && isGrounded)
        {
            Vector3 v = rb.linearVelocity;
            v.y = jumpVelocity;
            rb.linearVelocity = v;
        }

        jumpRequested = false;
    }

    private void HandleGlide()
    {
        if (!enableGlide) return;
        if (isGrounded) return;
        if (!jumpHeld) return;
        if (rb.linearVelocity.y > glideActivationThreshold) return;

        Vector3 v = rb.linearVelocity;
        v.y = Mathf.Lerp(v.y, glideVerticalTarget, 0.5f);
        rb.linearVelocity = v;
    }

    private void ApplyFallMultiplier()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Vector3.up * (Physics.gravity.y * (fallMultiplier - 1f)), ForceMode.Acceleration);
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
