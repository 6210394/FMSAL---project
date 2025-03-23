using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackAnimationData", menuName = "Scriptable Objects/AttackAnimationData")]
public class AttackAnimationData : ScriptableObject
{
    //This serves to give moves their specific hitboxes. Different hitboxes can be created by the animation using their index.

    public string animationTriggerName;

    public List<Hurtbox> hurtboxes;

    [System.Serializable]
    public struct Hurtbox
    {
        public GameObject hurtboxGameobject;

        public Vector3 hurtboxOffset;
        public Quaternion hurtboxRotationOffset;
        public Vector3 hurtboxScale;
    }
}
