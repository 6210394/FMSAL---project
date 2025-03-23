using UnityEngine;

public class ApproachAndAttack : IState
{
    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;
    private EnemyBlueprint _brawlerEnemy;

    bool hasAttacked = false;
    bool completedAttack;

    public ApproachAndAttack(EnemyBlueprint brawlerEnemy, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _brawlerEnemy = brawlerEnemy;
    }

    private void Start()
    {
        _combatController.OnHit.AddListener(CompleteAttack);
    }

    public void Tick()
    {
        if(!completedAttack)
        {
            if(!HasTarget())
            {
                return;
            }
            
            if(Vector3.Distance(_movementController.transform.position, _brawlerEnemy._target.transform.position) > 1)
            {
                Vector3 moveDir = (_brawlerEnemy._target.position - _movementController.transform.position).normalized;
                _movementController.MoveEnemyInDirection(moveDir, true);
            }
            else if (!hasAttacked)
            {
                hasAttacked = true;
                _combatController.combatScript.Attack(CombatScript.CombatActionType.LightMelee, 0);
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
        if(_brawlerEnemy._target)
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
