using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    public int damage = 1;
    public LayerMask destroyLayers;

    private Vector3 direction;

    void Start()
    {
        // Trouve le joueur au moment du tir
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            direction = (player.transform.position - transform.position).normalized;
        }
        else
        {
            direction = transform.forward;
        }

        // Auto-destruction après quelques secondes
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Si le projectile touche un layer "solide" -> destruction
        if (((1 << other.gameObject.layer) & destroyLayers) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // Si c’est le joueur, infliger des dégâts
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
