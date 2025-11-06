using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    public float acceleration = 10f;
    public float turnSpeed = 100f;
    public float maxSpeed = 10f;

    [Header("Saut et planement")]
    public float jumpForce = 7f;
    public float dragSpeed = -2f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;
    public float holdThreshold = 0.4f;   // temps de maintien avant planement

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool isGrounded;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Coroutine holdCoroutine;

    void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        if (jumpAction != null)
        {
            jumpAction.started += OnJumpStarted;
            jumpAction.canceled += OnJumpCanceled;
        }
    }

    void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.started -= OnJumpStarted;
            jumpAction.canceled -= OnJumpCanceled;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // --- Déplacement ---
        moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.forward * moveInput.y * acceleration;

        if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude < maxSpeed)
            rb.AddForce(move, ForceMode.Acceleration);

        transform.Rotate(Vector3.up, moveInput.x * turnSpeed * Time.fixedDeltaTime);

        // --- Saut ---
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }

        // --- Planement ---
        if (!isGrounded && jumpHeld && rb.linearVelocity.y < dragSpeed)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = Mathf.Lerp(vel.y, dragSpeed, 0.1f);
            rb.linearVelocity = vel;
        }
    }

    // --- Gestion des inputs de saut ---
    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
            jumpPressed = true;

        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        holdCoroutine = StartCoroutine(HoldDelayCoroutine());
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        jumpHeld = false;
        jumpPressed = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private IEnumerator HoldDelayCoroutine()
    {
        float t = 0f;
        while (t < holdThreshold)
        {
            if (!jumpAction.IsPressed()) yield break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        jumpHeld = true;
        holdCoroutine = null;
    }
}
