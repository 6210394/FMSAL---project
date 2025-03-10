using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public int _health = 3;
    public float _moveSpeed = 5;

    [Header("Stun Tolerance")]
    public int _maximumChainStun = 2;
    
    [Header("Attack Options")]
    public float _comfortRange = 5f;
    public float _detectionRange = 15f;
    public float _fieldOfViewAngle = -135f;

    [Header("Weapons")]
    public WeaponScript _mainWeapon;
    public WeaponScript _sidearm;
}
