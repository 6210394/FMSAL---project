using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    public float maxHealth = 3f;
    public float currentHealth;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        currentHealth = maxHealth;
    }

    public virtual void LoseHealth(float amount)
    {
        currentHealth -= amount;
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
