using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HoldableItem", menuName = "Combat/Weapon Type")]
public class WeaponScript : IObjectType
{
    public enum WeaponType {Melee, Gun, Junk}

    [Header("Object Details")]

        public bool isThrowable;
        public WeaponType weaponType;

    [Header("Weapon Stats")]
    
        public int damage; //Average health is 3-5
        public float stunTime;
    [Header("Melee Weapon")]

        //Melee
        public float swingTime;
        public float weaponReach; //How far will the player lunge (PLAYER ONLY)
        public float weaponTargetOffset;

        public List<AttackAnimationData> listOfAttacks;

    #region UNUSED
    [Header("Ranged Weapon")]
        //Guns
        public float rateOfFire; //RPM - Rounds per minute, example: 140RPM
        public float weaponAimAssistValue; //Size of the aim assist bubble in units, ~1
    [Space]
        public float reloadSpeed;
        public int maxAmmo;
        public int magSize;
        public int totalAmmoRemaining;
        public int currentAmmoInMag;
    #endregion
}
