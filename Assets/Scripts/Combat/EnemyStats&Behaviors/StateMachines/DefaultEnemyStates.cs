using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class DefaultEnemyStates : EnemyStateMachineBlueprint
{
    
    public Coroutine CircleTargetCoroutine;
    public Coroutine PatrolCoroutine;

    void Start()
    {
        enemyMovementController.SetPatrol();
        enemyMovementController.SetCircling();
    }

    public override void Update()
    {
        base.Update();
        if(enemyCombatController.seeksRetaliation)
        {
            SwitchToNextEvent(EVENT.ENTER);
            SwitchToNextState(STATE.ATTACKING);
            enemyCombatController.Retaliate();
        }
    }

    protected override void RunStateMachine()
    {
        if(!enemyCombatController.isDead)
        {
            switch (currentState)
            {
                case STATE.PATROL:
                    Patrol();
                    break;

                case STATE.MOVING:
                    MoveToPlayer();
                    break;

                case STATE.ATTACKING:
                    Attacking();
                    break;
                
                case STATE.CIRCLING:
                    Circling();
                    break;

                case STATE.RETREATING:
                    Retreat();
                    break;
            }
        }   
    }

    public void Patrol()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                enemyMovementController.givenMoveDestination = enemyMovementController.givenMoveDirection + transform.position;
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(!enemyCombatController.CheckForPlayersInDetectionRange())
                {   
                    enemyMovementController.MoveEnemyUntilReached(enemyMovementController.givenMoveDestination, false);

                    if(!enemyMovementController.movementScript.isMoving)
                    {
                        SwitchToNextEvent(EVENT.EXIT);
                    }
                }
                else
                {
                    SwitchToNextEvent(EVENT.EXIT);
                }
                break;
            }
            case EVENT.EXIT:
            {
                if(enemyCombatController.target)
                {
                    SwitchToNextState(STATE.MOVING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                else
                {
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
        }
    }

    public void Circling()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                if(enemyCombatController.target)
                {
                    SwitchToNextEvent(EVENT.UPDATE);
                }
                else
                {
                    SwitchToNextState(STATE.PATROL);
                }
                break;
            }
            case EVENT.UPDATE:
            {
                if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) > enemyCombatController.comfortRange + 3)
                {
                    SwitchToNextState(STATE.MOVING);
                    SwitchToNextEvent(EVENT.ENTER);
                }

                if(enemyCombatController.target)
                {
                    if(!enemyCombatController.target.movementScript.isDodging)
                    {
                        transform.LookAt(enemyCombatController.target.transform);
                    }
                    enemyMovementController.EnemyCirclingMovement(enemyCombatController.target.transform.position);
                }

                if(enemyCombatController.isPreparingAttack)
                {
                    SwitchToNextState(STATE.ATTACKING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            case EVENT.EXIT:
            {
                break;
            }
        }
    }

    public void MoveToPlayer()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }

            case EVENT.UPDATE:
            {
                if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) < enemyCombatController.detectionRange)
                {
                    if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) > enemyCombatController.comfortRange)
                    {
                        enemyMovementController.MoveEnemyInDirection(enemyCombatController.target.transform.position,true);
                        transform.LookAt(enemyCombatController.target.transform);
                    }
                    else
                    {
                        SwitchToNextEvent(EVENT.EXIT);
                    }
                }
                else
                {
                    SwitchToNextState(STATE.PATROL);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            
            case EVENT.EXIT:
            {
                SwitchToNextState(STATE.CIRCLING);
                SwitchToNextEvent(EVENT.ENTER);
                break;
            }
        }
    }

    public void Attacking()
    {
        switch (currentEvent)
        {
            case EVENT.ENTER:
            {

                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(Vector3.Distance(enemyCombatController.target.transform.position, transform.position) > 1)
                {
                    enemyMovementController.MoveEnemyInDirection(enemyCombatController.target.transform.position, false);
                }
                else
                {
                    enemyMovementController.isRetreating = true;
                }

                if(enemyMovementController.isRetreating)
                {
                    SwitchToNextState(STATE.RETREATING);
                    SwitchToNextEvent(EVENT.ENTER);
                }
                break;
            }
            case EVENT.EXIT:
            {
                SwitchToNextState(STATE.CIRCLING);                
                break;
            }
        }
    }

    public void Retreat()
    {
        switch(currentEvent)
        {
            case EVENT.ENTER:
            {   
                if(Random.Range(0,2) == 1)
                {
                    if(enemyCombatController.target)
                    {
                        SwitchToNextState(STATE.CIRCLING);
                        break;
                    }
                }
                enemyMovementController.isRetreating = true;
                SwitchToNextEvent(EVENT.UPDATE);
                break;
            }
            case EVENT.UPDATE:
            {
                if(!enemyMovementController.IsRetreating())
                {
                    SwitchToNextEvent(EVENT.EXIT);
                }
                else
                {
                    enemyMovementController.RetreatAwayUntilDistance(enemyCombatController.comfortRange, enemyCombatController.target.transform.position);
                }
                break;
            }
            case EVENT.EXIT:
            {
                if(enemyCombatController.target)
                {
                    SwitchToNextState(STATE.CIRCLING);
                }
                else
                {
                    SwitchToNextState(STATE.PATROL);
                }
                SwitchToNextEvent(EVENT.ENTER);
                break;
            }
        }
    }

    public IEnumerator MeleeAttack()
    {
        yield return null;
    }

    public IEnumerator CircleTarget()
    {
        yield return new WaitForSeconds(Random.Range(1, 4));
    }

    void OnDrawGizmos()
    {
        if (enemyCombatController == null) return;

        Gizmos.color = GetStateColor(currentState);
        Gizmos.DrawSphere(transform.position + Vector3.up * 2, 0.5f);
    }

    Color GetStateColor(STATE state)
    {
        switch (state)
        {
            case STATE.PATROL:
                return Color.green;
            case STATE.MOVING:
                return Color.blue;
            case STATE.ATTACKING:
                return Color.red;
            case STATE.CIRCLING:
                return Color.yellow;
            case STATE.RETREATING:
                return Color.magenta;
            default:
                return Color.white;
        }
    }
}
