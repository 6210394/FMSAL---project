using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackAnimationData", menuName = "Scriptable Objects/AttackAnimationData")]
public class AttackAnimationData : ScriptableObject
{
    //This serves to give animations/parts of combos their specific hitboxes. Different hitboxes can be created by the animation using their index.
    public float hitImpactCameraShakeAmplitude;
    public string animationTriggerName;
    public float animationEndCooldown; //keep this around animation length for expected results
    public ParticleEffect particleEffect;

    public List<Hurtbox> hurtboxes;

    [System.Serializable]
    public struct Hurtbox
    {
        public GameObject hurtboxGameobject;

        public Vector3 hurtboxOffset;
        public Quaternion hurtboxRotationOffset;
        public Vector3 hurtboxScale;
    }

    [System.Serializable]
    public struct ParticleEffect
    {
        public ParticleSystemScript particleEffectScript;

        public Vector3 particleOffset;
        public Quaternion particleRotationOffset;
        public Vector3 particleScale;
    }
}
