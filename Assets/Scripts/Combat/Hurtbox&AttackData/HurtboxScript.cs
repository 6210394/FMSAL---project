using UnityEngine;

public class HurtboxScript : MonoBehaviour
{
    public CombatScript.HitEventArgs _hitEventArgs;

    void OnTriggerEnter(Collider other)
    {
        SendMessageUpwards("OnTakeHit", _hitEventArgs);
    }
}
