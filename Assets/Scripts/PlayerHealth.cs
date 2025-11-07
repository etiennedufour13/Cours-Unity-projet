using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public Image[] hearts; // assigner les 3 icônes dans l’ordre

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHearts();

        if (currentHealth <= 0)
        {
            // ici tu pourras ajouter une logique de mort / respawn
            Debug.Log("Player is dead");
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = (i < currentHealth);
        }
    }
}
