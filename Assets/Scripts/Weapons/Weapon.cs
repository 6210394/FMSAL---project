using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponDamage;

    public bool isActiveWeapon;

    public bool isFirearm;
    public int weaponMaxMagazine;
    public int weaponCurrentMagazine;

    enum TYPE
    {
        MELEE, SIDEARM, GUN
    }
    
    public virtual void Attack()
    {

    }
}
