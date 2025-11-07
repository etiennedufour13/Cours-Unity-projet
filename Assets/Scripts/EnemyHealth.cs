using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 3;




    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
            Destroy(gameObject);
    }
}
