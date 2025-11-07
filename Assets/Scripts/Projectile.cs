using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    public float speed = 10f;

    public GameObject explosion;
    public LayerMask stopProjectile;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(1);
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }
        else if (other.transform.gameObject.layer == stopProjectile)
        {
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }
    }
}
