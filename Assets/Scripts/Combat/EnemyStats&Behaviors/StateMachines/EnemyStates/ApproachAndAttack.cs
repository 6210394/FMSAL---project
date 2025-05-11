using UnityEngine;

public class ApproachAndAttack : IState
{
    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;
    private EnemyBlueprint _enemyStates;

    bool hasAttacked = false;
    bool completedAttack;

    public ApproachAndAttack(EnemyBlueprint enemyStates, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _enemyStates = enemyStates;
    }

    public void Tick()
    {
        if(!completedAttack)
        {
            if(!HasTarget())
            {
                return;
            }
            
            if(Vector3.Distance(_movementController.transform.position, _enemyStates._target.transform.position) > _combatController.combatScript.targetDistanceOffset + 0.5f && !hasAttacked)
            {
                Vector3 adjustedPosition = _enemyStates._target.transform.position 
                + (_movementController.transform.position - _enemyStates._target.transform.position).normalized 
                * _combatController.combatScript.targetDistanceOffset;
                _movementController.MoveEnemyUntilReached(adjustedPosition, true);
            }
            else if (!hasAttacked)
            {
                hasAttacked = true;
                _combatController.combatScript.Attack(CombatScript.CombatActionType.LightMelee);
            }
            else
            {
                if(!_combatController.combatScript.attackIsAvailable)
                {
                    CompleteAttack();
                }
            }
        }
    }

    public void OnEnter()
    {
        completedAttack = false;
    }

    public void OnExit()
    {
        CompleteAttack();
    }

    public bool HasTarget()
    {
        if(_enemyStates._target)
        {
            return true;
        }
        return false;
    }

    public void CompleteAttack()
    {
        completedAttack = true;
        hasAttacked = false;
        _combatController.isAvailableForEnemyManager = true;
        _combatController.isPreparingAttack = false;
    }

    public Color GizmoColor()
    {
        return Color.red;
    }
}
