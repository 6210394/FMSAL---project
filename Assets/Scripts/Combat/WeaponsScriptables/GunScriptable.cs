using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class GunScript : WeaponStatsScriptable
{
    public float fireRate = 0.5f;
    public int ammoInventorySize = 10;
    public int magazineSize = 10;
    public int curentAmmo = 10;
}
