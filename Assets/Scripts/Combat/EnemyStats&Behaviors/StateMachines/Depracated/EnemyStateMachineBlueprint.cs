using System.Collections;
using UnityEngine;

public class EnemyStateMachineBlueprint : MonoBehaviour
{
    public bool isPaused = false;

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
    public EnemyMovementController enemyMovementController;
    public Animator animator;

    public virtual void Update()
    {
        if(!isPaused)
        {
            RunStateMachine();
        }
    }

    public virtual void Init()
    {
        enemyCombatController = GetComponent<EnemyCombatController>();
        enemyMovementController = GetComponent<EnemyMovementController>();
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

    public IEnumerator IWait(int waitTime)
    {
        if(!isPaused)
        {
            isPaused = true;
            yield return new WaitForSeconds(waitTime);
            isPaused = false;
        }
    }
}
