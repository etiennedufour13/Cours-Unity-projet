using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 5f;
    public int damage = 1;
    public LayerMask stopProjectile;
    public GameObject explosion;

    public AudioClip impact, playerHurt;
    private AudioSource audioSource;

    private Vector3 direction;



    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player)
        {
            direction = (player.transform.position - transform.position).normalized;
        }
        else
        {
            direction = transform.forward;
        }

        audioSource = GameObject.FindGameObjectWithTag("AudioEffects").GetComponent<AudioSource>();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit");

        //si sol/mur
        if ((stopProjectile.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            audioSource.PlayOneShot(impact, 1F);
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }

        //si player
        if (other.CompareTag("Player"))
        {
            audioSource.PlayOneShot(playerHurt, 1F);

            Debug.Log("hit player");
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(explo, 1);
            Destroy(gameObject);
        }
    }
}
