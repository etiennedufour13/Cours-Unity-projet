using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f;
    public float speed = 10f;

    public GameObject explosion;
    public LayerMask stopProjectile;

    public AudioClip impact, enemyHurt;
    private AudioSource audioSource;



    void Start()
    {
        Destroy(gameObject, lifetime);
        audioSource = GameObject.FindGameObjectWithTag("AudioEffects").GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        //si touche ennemie
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            audioSource.PlayOneShot(enemyHurt, 1F);
            enemy.TakeDamage(1);
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }
        //si touche mur/sol
        else if (other.transform.gameObject.layer == stopProjectile)
        {
            audioSource.PlayOneShot(impact, 1F);
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }
    }
}
