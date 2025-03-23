using UnityEngine;

public class HurtboxScript : MonoBehaviour
{
    public CombatScript.HitEventArgs _hitEventArgs;

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<HealthScript>() != null)
        {
            
        }
        SendMessage("OnTakeHit", _hitEventArgs);
        Debug.Log(_hitEventArgs.damageSource);
    }
}
