using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootForce = 20f;

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.currentState != GameState.Gameplay) return;

        if (value.isPressed)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);
        }
    }
}
