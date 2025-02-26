using UnityEngine;

[CreateAssetMenu(fileName = "HoldableItem", menuName = "Scriptable Objects/Weapon")]
public class WeaponScript : IObjectType
{
    [Header("Object Details")]

        public bool isThrowable;

        public bool isMelee;
        public bool isGun;

    [Header("Weapon Stats")]
    
        public int damage;
        public float stunTime;
    [Header("Melee Weapon")]
        //Melee
        public float swingTime;
        public float weaponReach;
    [Header("Ranged Weapon")]
        //Guns
        public float rateOfFire;
        public float weaponAimAssistValue;
        public float reloadSpeed;
        public int maxAmmo;
        public int magSize;
        public int totalAmmoRemaining;
        public int currentAmmoInMag;
}
