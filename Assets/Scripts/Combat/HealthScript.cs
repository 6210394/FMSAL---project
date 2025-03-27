using UnityEngine;
using UnityEngine.Events;

public class HealthScript : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth = 3;
    public bool isDead;

    public UnityEvent<CombatScript.HitEventArgs> OnTakeDamage;
    public UnityEvent OnDeath;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.V))
        {
            CombatScript.HitEventArgs hitEventArgs;
            hitEventArgs.damageSource = null;
            hitEventArgs.damageReceived = 1;
            hitEventArgs.stunDuration = 0;
            TakeDamage(hitEventArgs);
        }
    }

    // Method to take damage
    public void TakeDamage(CombatScript.HitEventArgs hitEventArgs)
    {
        if(transform != hitEventArgs.damageSource && !isDead)
        {
            currentHealth -= hitEventArgs.damageReceived;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else
            {
                OnTakeDamage.Invoke(hitEventArgs);
            }
        }        
        else
        {
            Debug.Log(name + ": I am the source");
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
        Debug.Log("death message sent");
        isDead = true;
        OnDeath.Invoke();
    }
}
