using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public int health = 3;
    public float moveSpeed = 5;
    public float entityWeight = 1f;
    public float detectionRange = 20;
    public float attackRange = 2;
}
