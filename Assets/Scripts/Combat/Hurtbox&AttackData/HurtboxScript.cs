using UnityEngine;

public class HurtboxScript : MonoBehaviour
{
    public CombatScript.HitEventArgs _hitEventArgs;

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<HealthScript>() != null)
        {
            other.GetComponent<HealthScript>().TakeDamage(_hitEventArgs);
        }
        else
        {
            Debug.Log(this + " has not health script!!");
        }
    }
}
