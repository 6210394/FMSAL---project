using UnityEngine;
using UnityEngine.Events;

public class HealthScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 3;

    UnityEvent OnTakeDamage;
    UnityEvent OnDeath;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            OnTakeDamage.Invoke();
        }
    }

    // Method to heal
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    // Method to handle death
    void Die()
    {
        Debug.Log("Character is dead.");
        OnDeath.Invoke();
        // Add additional logic for when the character dies, such as playing a death animation
    }
}
