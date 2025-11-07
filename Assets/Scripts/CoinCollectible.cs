using UnityEngine;
using UnityEngine.Audio;

public class CoinCollectible : MonoBehaviour
{
    public AudioClip coin;
    private AudioSource audioSource;



    private void Start()
    {
        audioSource = GameObject.FindGameObjectWithTag("AudioEffects").GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        inventory.AddCoin();
        audioSource.PlayOneShot(coin, .7F);
        Destroy(gameObject);
    }
}
