using UnityEngine;

public class TakeDamage : IState
{
    BrawlerEnemy _brawlerEnemy;

    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;

    int currentChainStun = 0;

    public TakeDamage(BrawlerEnemy brawlerEnemy, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _brawlerEnemy = brawlerEnemy;
    }

    public void OnEnter()
    {
        RecieveHit();
    }

    public void OnExit()
    {

    }

    public void Tick()
    {

    }

    void RecieveHit()
    {
        if(_combatController.isDead)
        {
            _brawlerEnemy.Death();
            return;
        }

        _animator.SetTrigger("RecieveHit");
        _movementController.movementScript.KnockBack(0.3f, 0.1f, _brawlerEnemy._damageSource);
        if(!_combatController.combatScript.stunImmune)
        {
            currentChainStun += 1;
            _combatController.combatScript.GetStunned(_brawlerEnemy._stunDuration);
        }

        Retaliate();
        _brawlerEnemy._hasTakenHit = false;
    }

    public bool Retaliate()
    {
        if(currentChainStun >= _combatController.maximumChainStun && _combatController.combatScript.debugCanAttack)
        {
            _combatController.combatScript.stunImmune = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public Color GizmoColor()
    {
        return Color.green;
    }
}
