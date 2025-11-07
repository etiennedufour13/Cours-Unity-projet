using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Références")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootForce = 20f;

    [Header("Audio")]
    public AudioClip shoot;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GameObject.FindGameObjectWithTag("AudioEffects").GetComponent<AudioSource>();
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.currentState != GameState.Gameplay) return;

        if (value.isPressed)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            audioSource.PlayOneShot(shoot, .7F);
            if (rb != null)
                rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);
        }
    }
}
