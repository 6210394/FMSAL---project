using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


[RequireComponent(typeof(CombatScript))]
[RequireComponent(typeof(PlayerController))]
public class PlayerCombatController : MonoBehaviour
{
    public MovementScript movementScript;
    public CombatScript combatScript;

    public EnemyScript lockedTarget;

    private EnemyManager enemyManager;
     EnemyDetection enemyDetection;

    public UnityEvent<EnemyScript> OnPunch;
    public UnityEvent<EnemyScript> OnTrajectory;

    public float punchRange = 4f;
    public float punchDuration = 0.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyDetection = GetComponent<EnemyDetection>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Punch(punchRange);
        }
    }

    void Punch(float range)
    {
        if(!combatScript.canAttack || combatScript.isAttacking || combatScript.isAttacking)
        {
            return;
        }
        combatScript.Attack(CombatScript.AttackType.Punch);
        lockedTarget = enemyDetection.CurrentTarget();
        if(lockedTarget != null)
        {
            Debug.Log("Punching");
            transform.DOLookAt(lockedTarget.transform.position, punchDuration);
            if(TargetDistance(lockedTarget) < range)
            {
                transform.DOMove(lockedTarget.transform.position, punchDuration);
                OnPunch.Invoke(lockedTarget);
            }
        }
    }

    public void DamageEvent()
    {
        // Implementation of DamageEvent
    }
    

    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }
}
