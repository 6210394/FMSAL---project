using UnityEngine;

public class EnemyStateMachineBlueprint : MonoBehaviour
{
    public float attackRange = 2;
    public float detectionRange = 20;

    public float comfortDistance = 5f;

    public enum STATE { PATROL, MOVING, ATTACKING, CIRCLING, RETREATING}
    public enum EVENT { ENTER, UPDATE, EXIT }
    
    public STATE currentState;
    public EVENT currentEvent;

    public EnemyStateMachineBlueprint()
    {
        currentState = STATE.PATROL;
        currentEvent = EVENT.ENTER;
    }

    public EnemyCombatController enemyCombatController;
    public Animator animator;

    public virtual void Update()
    {
        if(!enemyCombatController.isPaused)
        {
            RunStateMachine();
        }
    }

    public virtual void Init()
    {
        enemyCombatController = GetComponent<EnemyCombatController>();
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void RunStateMachine()
    {
        
    }

    protected void SwitchToNextState(STATE nextState)
    {
        currentState = nextState;
    }
    protected void SwitchToNextEvent(EVENT nextEvent)
    {
        currentEvent = nextEvent;
    }

    protected bool CheckIfObjectInSight(GameObject gameObject)
        {
            
            Vector3 directionToTargetObject = gameObject.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, directionToTargetObject, out hit))
            {
                if (hit.collider.gameObject.tag != "Player")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

}
