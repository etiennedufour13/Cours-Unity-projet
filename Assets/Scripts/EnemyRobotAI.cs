using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class EnemyRobotAI : MonoBehaviour
{
    [Header("Tir")]
    public Transform target;
    public float engageDistance = 8f;
    public float disengageDistance = 10f;
    public float fireInterval = 2f;
    public string attackAnimatorBool = "attackOn";
    public UnityEvent onFire;

    [Header("Orientation head")]
    public Transform headTransform;
    public float headSmoothTime = 0.08f;
    public float headMaxDeltaDegPerFrame = 0f;

    public bool rotateHeadOnlyWhenAttacking = true;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Audio")]
    public AudioClip[] reveils;
    public AudioClip shoot;
    private AudioSource audioSource;


    private Animator animator;
    private float fireTimer = 0f;
    private bool isAttacking = false;
    private float headYawVel = 0f;



    private void Start()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        animator = GetComponent<Animator>();

        if (headTransform == null)
        {
            Transform possible = transform.Find("Head");
            if (possible != null) headTransform = possible;
        }

        audioSource = GameObject.FindGameObjectWithTag("AudioEffects").GetComponent<AudioSource>();
    }

    private void Update()
    {

        float sqrDist = (target.position - transform.position).sqrMagnitude;
        float engageSqr = engageDistance * engageDistance;
        float disengageSqr = disengageDistance * disengageDistance;


        if (!isAttacking && sqrDist <= engageSqr)
        {
            EnterAttackMode();
        }
        else if (isAttacking && sqrDist >= disengageSqr)
        {
            ExitAttackMode();
        }

        if (isAttacking)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }

        if (headTransform != null && (!rotateHeadOnlyWhenAttacking || isAttacking))
        {
            RotateHeadTowardsTarget();
        }
    }

    private void EnterAttackMode()
    {
        isAttacking = true;
        fireTimer = 0f; 
        if (animator != null && !string.IsNullOrEmpty(attackAnimatorBool))
            animator.SetBool(attackAnimatorBool, true);

        audioSource.PlayOneShot(reveils[Random.Range(0, 3)], .7F);
    }

    private void ExitAttackMode()
    {
        isAttacking = false;
        if (animator != null && !string.IsNullOrEmpty(attackAnimatorBool))
            animator.SetBool(attackAnimatorBool, false);
    }

    private void Fire()
    {
        if (projectilePrefab == null || firePoint == null) return;

        audioSource.PlayOneShot(shoot, .7F);
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    private void RotateHeadTowardsTarget()
    {
        Vector3 dir = target.position - headTransform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= Mathf.Epsilon) return;

        Quaternion targetWorldRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        float desiredY = targetWorldRot.eulerAngles.y;

        float currentY = headTransform.eulerAngles.y;

        float newY = Mathf.SmoothDampAngle(currentY, desiredY, ref headYawVel, headSmoothTime, Mathf.Infinity, Time.deltaTime);

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

    //visualisation
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engageDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, disengageDistance);
    }
}
