using System;
using UnityEngine;

public class BrawlerEnemy : EnemyBlueprint
{
    protected override void Start()
    {
        base.Start();

        //Create the states that will compose my AI
        var takeDamage = new TakeDamage(this, _movementController, _combatController, _animator);
        var patrol = new Patrol(this, _movementController, _animator);
        var circlingPlayer = new CirclingPlayer(this, _movementController, _combatController, _animator);
        var approachAndAttack = new ApproachAndAttack(this, _movementController, _combatController, _animator);
        var searchAroundGivenArea = new SearchAroundGivenArea(this, _movementController);
        var retreat = new Retreat(this, _movementController);

        //Create the transitions with their condition
        At(patrol, circlingPlayer, PlayerInDetectionRange());
        At(circlingPlayer, approachAndAttack, PreparingAttack());
        At(takeDamage, approachAndAttack, Retaliate());
        At(takeDamage, searchAroundGivenArea, OutOfHit());
        At(searchAroundGivenArea, circlingPlayer, PlayerInDetectionRange());
        At(searchAroundGivenArea, patrol, FinishedSearching());
        At(circlingPlayer, approachAndAttack, PreparingAttack());
        At(retreat, circlingPlayer, FinishedRetreating());

        _stateMachine.AddAnyTransition(takeDamage, TookHit());
        _stateMachine.AddAnyTransition(retreat, WantsToRetreat());
        _stateMachine.AddAnyTransition(searchAroundGivenArea, PlayerLeftDetectionRange());
        
        _combatController.combatScript.healthScript.OnTakeDamage.AddListener((CombatScript.HitEventArgs hitEventArgs) => OnTakeHit(hitEventArgs.stunDuration, hitEventArgs.damageSource));
        _combatController.combatScript.OnAttackCompleted.AddListener(RetreatAfterHit);
        _combatController.combatScript.healthScript.OnDeath.AddListener(Death);

        //Begin at Patrol
        _stateMachine.SetState(patrol);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> PlayerInDetectionRange() => () => CheckForPlayersInDetectionRange();
        Func<bool> PlayerLeftDetectionRange() => () => LoseSightOfTarget();
        Func<bool> PreparingAttack() => () => _combatController.isPreparingAttack == true;
        Func<bool> TookHit() => () => _hasTakenHit;
        Func<bool> OutOfHit() => () => !_hasTakenHit;
        Func<bool> Retaliate() => () => takeDamage.Retaliate();
        Func<bool> FinishedSearching() => () => searchAroundGivenArea.HasTimerReachedMax();
        Func<bool> WantsToRetreat() => () => wantsToRetreat;
        Func<bool> FinishedRetreating() => () => retreat.HasRetreated();
    }

    // Update is called once per frame
    void Update()
    {
        if(_stateMachine != null)
        {            
            _stateMachine.Tick();
        }
    }

    private void RetreatAfterHit()
    {
        wantsToRetreat = true;
    }

    private bool LoseSightOfTarget()
    {
        if(!CheckForPlayersInDetectionRange())
        {
            if(_target != null)
            {
                _damageSource = _target.transform.position;
                _target = null;

                if(_combatController.enemyManager.availableEnemies.Contains(this) || !_combatController.isAvailableForEnemyManager)
                {
                    _combatController.enemyManager.RemoveEnemy(this);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        else return false;
    }

}
