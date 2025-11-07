using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class EnemyRobotAI : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("If left empty, the script will look for the first GameObject tagged 'Player' on Start.")]
    public Transform target;

    [Header("Engagement Distances")]
    [Tooltip("Distance at or below which the enemy enters attack mode.")]
    public float engageDistance = 8f;
    [Tooltip("Distance at or above which the enemy exits attack mode (should be >= engageDistance).")]
    public float disengageDistance = 10f;

    [Header("Attack / Fire")]
    [Tooltip("Seconds between projectile launches while attacking.")]
    public float fireInterval = 2f;
    [Tooltip("Name of the boolean parameter in the Animator used to signal attack state.")]
    public string attackAnimatorBool = "attackOn";
    [Tooltip("UnityEvent invoked each time the enemy 'fires'. Hook projectile spawn logic here in the inspector.")]
    public UnityEvent onFire;

    [Header("Head / Orientation")]
    [Tooltip("Transform of the head (child) that will yaw toward the player. If null, will try to find a child named 'Head'.")]
    public Transform headTransform;
    [Tooltip("Smooth time for head yaw rotation (seconds).")]
    public float headSmoothTime = 0.08f;
    [Tooltip("Optional: clamp maximum yaw change per frame (deg) to avoid snapping; set to 0 to disable.")]
    public float headMaxDeltaDegPerFrame = 0f;

    [Header("Behavior")]
    [Tooltip("If true, the head will only rotate while in attack mode; otherwise it rotates continuously.")]
    public bool rotateHeadOnlyWhenAttacking = true;

    [Header("Projectile System")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    // internals
    private Animator animator;
    private float fireTimer = 0f;
    private bool isAttacking = false;
    private float headYawVel = 0f;

    private void Start()
    {
        // find target if not assigned
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        // animator on this GameObject
        animator = GetComponent<Animator>();

        // find head if not assigned
        if (headTransform == null)
        {
            Transform possible = transform.Find("Head");
            if (possible != null) headTransform = possible;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            // nothing to track
            return;
        }

        float sqrDist = (target.position - transform.position).sqrMagnitude;
        float engageSqr = engageDistance * engageDistance;
        float disengageSqr = disengageDistance * disengageDistance;

        // Hysteresis: enter when <= engageDistance, exit when >= disengageDistance
        if (!isAttacking && sqrDist <= engageSqr)
        {
            EnterAttackMode();
        }
        else if (isAttacking && sqrDist >= disengageSqr)
        {
            ExitAttackMode();
        }

        // If attacking, handle firing timer
        if (isAttacking)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }

        // Head rotation: only when desired (attacking or continuous), otherwise keep last rotation
        if (headTransform != null && (!rotateHeadOnlyWhenAttacking || isAttacking))
        {
            RotateHeadTowardsTarget();
        }
    }

    private void EnterAttackMode()
    {
        isAttacking = true;
        fireTimer = 0f; // reset so we can fire immediately (optional)
        if (animator != null && !string.IsNullOrEmpty(attackAnimatorBool))
            animator.SetBool(attackAnimatorBool, true);
    }

    private void ExitAttackMode()
    {
        isAttacking = false;
        if (animator != null && !string.IsNullOrEmpty(attackAnimatorBool))
            animator.SetBool(attackAnimatorBool, false);
        // head rotation is intentionally left as-is (stays where it was)
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    private void RotateHeadTowardsTarget()
    {
        // Compute direction from head to target, only on horizontal plane
        Vector3 dir = target.position - headTransform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= Mathf.Epsilon) return;

        // Desired world yaw to face player
        Quaternion targetWorldRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        float desiredY = targetWorldRot.eulerAngles.y;

        // Current head world yaw
        float currentY = headTransform.eulerAngles.y;

        // Smoothly interpolate yaw using SmoothDampAngle
        float newY = Mathf.SmoothDampAngle(currentY, desiredY, ref headYawVel, headSmoothTime, Mathf.Infinity, Time.deltaTime);

        // Optionally clamp the delta to avoid frame-snapping
        if (headMaxDeltaDegPerFrame > 0f)
        {
            float delta = Mathf.DeltaAngle(currentY, newY);
            delta = Mathf.Clamp(delta, -headMaxDeltaDegPerFrame, headMaxDeltaDegPerFrame);
            newY = currentY + delta;
        }

        Vector3 headEuler = headTransform.eulerAngles;
        headEuler.y = newY;
        headTransform.eulerAngles = headEuler;
    }

    private void OnDrawGizmosSelected()
    {
        // visualize engagement radii
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engageDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, disengageDistance);
    }
}
