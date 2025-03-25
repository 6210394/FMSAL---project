using UnityEngine;

public class TakeDamage : IState
{
    EnemyBlueprint _enemyStates;

    private EnemyMovementController _movementController;
    private EnemyCombatController _combatController;
    private Animator _animator;

    int currentChainStun = 0;

    public TakeDamage(EnemyBlueprint enemyStates, EnemyMovementController movementController, EnemyCombatController combatController, Animator animator)
    {
        _movementController = movementController;
        _combatController = combatController;
        _animator = animator;
        _enemyStates = enemyStates;
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
            _enemyStates.Death();
            return;
        }
       
        _animator.SetTrigger("RecieveHit");
        if(_enemyStates._damageSource != null)
        {
            _movementController.movementScript.KnockBack(0.3f, 0.1f, _enemyStates._damageSource);
        }
        if(!_combatController.combatScript.stunImmune)
        {
            currentChainStun += 1;
            _combatController.combatScript.Stun(_enemyStates._stunDuration);
        }

        Retaliate();
        _enemyStates._hasTakenHit = false;
    }

    public bool Retaliate()
    {
        if(currentChainStun >= _combatController.maximumChainStun && _combatController.combatScript.ultimateCanAttack)
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
