using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEntityScript : CombatEntity
{
    public Animator anim;
    public Rigidbody rb;

    public AutomaticMovementScript autoMove;

    public bool moveDebugBool = false;
    public List<GameObject> players = new List<GameObject>();


    [Tooltip ("If true, the enemy will patrol around its spawn point. Otherwise, it will wander freely.")]
    public bool tiedPatrol = true;
    public float patrolRangeFromSpawn = 10f;

    public float detectionRange = 15f;
    public float fieldOfViewAngle = -135f;
    public bool hasTarget = false;

    public float deathTime = 1f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        autoMove = GetComponent<AutomaticMovementScript>();
        players = GameManager.instance.players;
    }

    void Update()
    {
        if(moveDebugBool)
        {
            if(autoMove.canMove)
            {
                autoMove.MoveWithNavMesh(players[0].transform.position);
            }
        }
        if(!hasTarget)
        {
            Debug.Log("No target");
            foreach (GameObject player in players)
            {
                if (IsPlayerInDetectionRange(player.transform) && IsPlayerInFront(player.transform))
                {
                    LockOnToPlayer(player.transform);
                    hasTarget = true;
                    return;
                }
            }
        }
        else
        {
            if(IsPlayerInDetectionRange(autoMove.targetObject.transform) && IsPlayerInFront(autoMove.targetObject.transform))
            {
                LockOnToPlayer(autoMove.targetObject.transform);
            }
            else
            {
                hasTarget = false;
            }
        }
        
    }
    
    bool IsPlayerInDetectionRange(Transform playerPosition)
    {   
        return Vector3.Distance(transform.position, playerPosition.position) <= detectionRange;
    }

    bool IsPlayerInFront(Transform playerPosition)
    {
        Vector3 directionToPlayer = (playerPosition.position- transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle <= fieldOfViewAngle / 2;
    }

    void LockOnToPlayer(Transform player)
    {
        autoMove.targetObject = player.gameObject;
        autoMove.MoveWithNavMesh(autoMove.targetObject.transform.position);
    }

    public override void LoseHealth(float amount)
    {
        base.LoseHealth(amount);
        anim.SetTrigger("TakeDamage");
    }

    public override void Die()
    {
        rb.constraints = RigidbodyConstraints.None; //fun basic ragdoll
        Destroy(gameObject, deathTime);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 frontRay = transform.position + transform.forward * patrolRangeFromSpawn;
        Vector3 leftRay = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, leftRay);
        Gizmos.DrawRay(transform.position, rightRay);
    }

}
