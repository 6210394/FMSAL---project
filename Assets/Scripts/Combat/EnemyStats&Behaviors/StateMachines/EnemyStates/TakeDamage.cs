using UnityEngine;
using UnityEngine.UIElements;

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
        Debug.Log("Is on TakeDamage step");
        RecieveHit(_enemyStates._damageSource);
    }

    public void OnExit()
    {
        
    }

    public void Tick()
    {
        if(!_combatController.combatScript.isStunned)
        {
            _enemyStates._hasTakenHit = false;
        }
    }

    void RecieveHit(Vector3 hitOrigin)
    {
        if(!_combatController.combatScript.healthScript.isDead)
        {
            _animator.SetTrigger("RecieveHit");
        }
        
        if(_enemyStates._damageSource != null)
        {
            _movementController.movementScript.Knockback(0.5f, hitOrigin, 1f);
            _combatController.isPreparingAttack = false;
        }
        if(!_combatController.combatScript.stunImmune)
        {
            currentChainStun += 1;
            _combatController.combatScript.Stun(_enemyStates._stunDuration);
        }

        //Retaliate();
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
