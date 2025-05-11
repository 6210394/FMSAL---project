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
    bool highlighted = false;

    [Header("Stun Tolerance")]
    public int maximumChainStun = 2;
    
    [Header("Attack Options")]
    public float comfortRange = 5f;
    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;

    [Header("Combat Booleans")]
    public bool isPreparingAttack = false;
    public bool isAvailableForEnemyManager = true;

    //References
    public EnemyManager enemyManager;
    public EnemyMovementController enemyMovementController;
    public CombatScript combatScript;

    [Header("Player References")]
    public GameObject[] players;
    public List<EnemyDetectionManager> playerEnemyDetections = new List<EnemyDetectionManager>();
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
        players = GameObject.FindGameObjectsWithTag("Player");

        //players[0].GetComponent<EnemyDetectionManager>().OnTargetSelected.AddListener(MarkTargetVisuals);
        //players[0].GetComponent<EnemyDetectionManager>().OnTargetSelected.AddListener(ClearTargetVisuals);
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

    public void Die()
    {   
        foreach(EnemyDetectionManager enemyDetection in playerEnemyDetections)
        {
            if(enemyDetection.CurrentTarget() == this)
            {
                enemyDetection.SetCurrentTarget(null);
            }
        }
        
        combatScript.ClearHurtboxes();

        ClearTargetVisuals();
        enemyMovementController.movementScript.navMeshAgent.ResetPath();
        combatScript.ultimateCanAttack = false;
        enemyManager.SetEnemyAvailiability(this, false);
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

    }

    public void MarkTargetVisuals()
    {
        if(!highlighted)
        {
            highlighted = true;

            foreach(SkinnedMeshRenderer meshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(meshRenderer.gameObject.CompareTag("HighlightableMaterial"))
                {
                    Material material = meshRenderer.material;
                    if (material.HasProperty("_OutlineOpacity"))
                    {
                        material.SetFloat("_OutlineOpacity", 3f);
                    }
                }
            }
        }
    }

    public void ClearTargetVisuals()
    {
        if(highlighted)
        {
            highlighted = false;
            
            foreach(SkinnedMeshRenderer meshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(meshRenderer.gameObject.CompareTag("HighlightableMaterial"))
                {
                    Material material = meshRenderer.material;
                    if (material.HasProperty("_OutlineOpacity"))
                    {
                        material.SetFloat("_OutlineOpacity", 0f);
                    }
                }
            }
        }
        
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
        return !combatScript.healthScript.isDead;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    #endregion
}
