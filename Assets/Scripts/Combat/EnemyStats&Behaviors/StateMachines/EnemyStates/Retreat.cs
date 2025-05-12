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
            Vector3 lookAtTarget = _enemyStates._target.transform.position;
            lookAtTarget.y = _enemyStates.transform.position.y;
            _movementController.MoveEnemyInDirection(retreatDirection, lookAtTarget, false);
            return;
        }
        else
        {
            Debug.Log("Not too close to target!");
        }
    }

    bool IsTooClose()
    {
        if(_enemyStates._target == null)
        {
            Debug.Log("NO TARGET WUT DA HELL");
            return false;
        }
        if(Vector3.Distance(_enemyStates._target.position, _movementController.transform.position) < _enemyStates._combatController.comfortRange)
        {
            Debug.Log("Too close to target!");
            return true;
        }
        else
        {
            hasRetreated = true;
            Debug.Log("Has Retreated!");
            return false;
        }
    }

    public bool HasRetreated()
    {
        return hasRetreated;
    }

    public Color GizmoColor()
    {
        return Color.yellow;
    }
}
