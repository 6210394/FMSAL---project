using UnityEngine;

[CreateAssetMenu(fileName = "HoldableItem", menuName = "Scriptable Objects/Weapon")]
public class WeaponScript : IObjectType
{
    public enum WeaponType {Melee, Gun, Junk}

    [Header("Object Details")]

        public bool isThrowable;
        public WeaponType weaponType;

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
    [Space]
        public float reloadSpeed;
        public int maxAmmo;
        public int magSize;
        public int totalAmmoRemaining;
        public int currentAmmoInMag;
}
