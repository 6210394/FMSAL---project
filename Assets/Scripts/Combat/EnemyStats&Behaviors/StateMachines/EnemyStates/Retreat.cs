using UnityEngine;

public class Retreat : IState
{
    EnemyBlueprint _enemyStates;

    private EnemyMovementController _movementController;

    bool hasRetreated = false;

    public Retreat(EnemyBlueprint enemyStates, EnemyMovementController movementController)
    {
        _movementController = movementController;
        _enemyStates = enemyStates;
    }

    public void OnEnter()
    {
        hasRetreated = false;
        _enemyStates.wantsToRetreat = false;

        if(Random.Range(1, 4) == 1) //25% chance of the brawler enemy sticking to you
        {
            hasRetreated = true;
        }
        
    }

    public void OnExit()
    {
        _enemyStates.ResetAnimator();
        hasRetreated = false;
    }

    public void Tick()
    {
        if(IsTooClose())
        {
            Vector3 retreatDirection = -_movementController.transform.forward; // Move backwards locally
            _movementController.MoveEnemyInDirection(retreatDirection, false);

            Vector3 lookAtTarget = _enemyStates._target.transform.position;
            lookAtTarget.y = _enemyStates.transform.position.y;
            _movementController.transform.LookAt(lookAtTarget);
            return;
        }
    }

    bool IsTooClose()
    {
        if(Vector3.Distance(_enemyStates._target.position, _movementController.transform.position) < 3)
        {
            return true;
        }
        else
        {
            hasRetreated = true;
            return false;
        }
    }

    public bool HasRetreated()
    {
        Debug.Log("Has Retreated!!");
        return hasRetreated;
    }

    public Color GizmoColor()
    {
        return Color.yellow;
    }
}
