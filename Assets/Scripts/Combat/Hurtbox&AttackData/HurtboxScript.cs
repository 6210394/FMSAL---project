using Unity.Cinemachine;
using UnityEngine;

public class HurtboxScript : MonoBehaviour
{
    public CombatScript.HitEventArgs _hitEventArgs;
    public AttackAnimationData.ParticleEffect particleEffect;

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<HealthScript>() != null && other.transform != _hitEventArgs.damageSource)
        {
            other.GetComponent<HealthScript>().TakeDamage(_hitEventArgs);
            CinemachineShake.Instance.ShakeCamera(_hitEventArgs.cameraShakeAmplitude, 0.1f);
            if(particleEffect.particleEffectScript != null)
            {
                Vector3 hitPosition = other.ClosestPointOnBounds(transform.position);
                Vector3 effectPosition = hitPosition + particleEffect.particleOffset;
                Instantiate(particleEffect.particleEffectScript, effectPosition, Quaternion.identity);
            }
        }
        else
        {
            Debug.Log(this + " has no health script!!");
        }
    }
}
