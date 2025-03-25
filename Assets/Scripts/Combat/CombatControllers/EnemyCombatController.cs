using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(EnemyMovementController))]
public class EnemyCombatController : MonoBehaviour
{
    [Header("States")]
    public bool seeksRetaliation = false;

    [Header("Stun Tolerance")]
    public int maximumChainStun = 2;
    
    [Header("Attack Options")]
    public float comfortRange = 5f;
    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    [Header("Combat Booleans")]
    public bool isDead = false;
    public bool isPreparingAttack = false;
    public bool isAvailableForEnemyManager = true;

    //References
    private EnemyManager enemyManager;
    public EnemyMovementController enemyMovementController;
    public CombatScript combatScript;

    [Header("Player References")]
    public List<GameObject> players = new List<GameObject>();
    public List<EnemyDetection> playerEnemyDetections = new List<EnemyDetection>();
    public Transform target;

    [Header("Debug Tools")]
    public bool moveDebugBool = false;

    void Awake()
    {
        combatScript = GetComponent<CombatScript>();
        enemyMovementController = GetComponent<EnemyMovementController>();
    }

    void Start()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
        combatScript.healthScript.OnTakeDamage.AddListener((CombatScript.HitEventArgs hitEventArgs) => OnTakeHit(hitEventArgs));
        combatScript.healthScript.OnDeath.AddListener(Die);
    }
    
    public void OnTakeHit(CombatScript.HitEventArgs hitEventArgs)
    {
        if(Vector3.Distance(hitEventArgs.damageSource.position, transform.position) <= detectionRange)
        {
            if(hitEventArgs.damageSource)
            {
                target = hitEventArgs.damageSource;
            }
        }
    }

    public void Retaliate()
    {
        isAvailableForEnemyManager = false;
        Attack();
    }

    /*
    public void DealDamageEvent()
    {
        target.SendMessage("OnTakeHit", combatScript.BuildAttack(combatScript.attackDamage, combatScript.meleeStunDuration, transform));
        OnHit.Invoke();
    }
    */

    public void Attack()
    {
        if(combatScript.ultimateCanAttack)
        {
            isPreparingAttack = true;
        }
        else
        {
            isPreparingAttack = false;
            Debug.Log(this + " CAN'T ATTACK BECAUSE OF DEBUG");
        }
    }

    void Die()
    {   
        isDead = true;

        foreach(EnemyDetection enemyDetection in playerEnemyDetections)
        {
            enemyDetection.SetCurrentTarget(null);
        }

        enemyManager.SetEnemyAvailiability(this, false);
    }

    public void OnDrawGizmos()
    {
        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return combatScript.healthScript.currentHealth > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    #endregion

}
