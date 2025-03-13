using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof (EnemyCombatController))]
[RequireComponent(typeof (EnemyMovementController))]

public class BrawlerEnemy : EnemyBlueprint
{

    protected override void Start()
    {
        base.Start();

        //Create the states that will compose my AI
        var takeDamage = new TakeDamage(this, _movementController, _combatController, _animator);
        var patrol = new Patrol(this, _movementController, _animator);
        var circlingPlayer = new CirclingPlayer(this, _movementController, _combatController, _animator);
        var approachAndPunch = new ApproachAndPunch(this, _movementController, _combatController, _animator);
        var searchDamageSourceArea = new SearchDamageSourceArea(this, _movementController);

        //Create the transitions with their condition
        At(patrol, circlingPlayer, PlayerInDetectionRange());
        At(circlingPlayer, approachAndPunch, ReadyToPunch());
        At(takeDamage, approachAndPunch, Retaliate());
        At(takeDamage, searchDamageSourceArea, OutOfHit());
        At(searchDamageSourceArea, circlingPlayer, PlayerInDetectionRange());
        At(searchDamageSourceArea, patrol, FinishedSearching());
        At(circlingPlayer, approachAndPunch, ReadyToPunch());

        _stateMachine.AddAnyTransition(takeDamage, TookHit());
        
        _combatController.OnDamage.AddListener((stunTime, damageSource) => OnTakeHit(stunTime, damageSource));

        //Begin at Patrol
        _stateMachine.SetState(patrol);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> PlayerInDetectionRange() => () => CheckForPlayersInDetectionRange();
        Func<bool> ReadyToPunch() => () => _isReadyToAttack == true;
        Func<bool> TookHit() => () => _hasTakenHit;
        Func<bool> OutOfHit() => () => !_hasTakenHit;
        Func<bool> Retaliate() => () => takeDamage.Retaliate();
        Func<bool> FinishedSearching() => () => searchDamageSourceArea.HasTimerReachedMax();
    }

    // Update is called once per frame
    void Update()
    {
        if(_stateMachine != null)
        {
            _stateMachine.Tick();
        }
    }

    private void OnDrawGizmos()
    {
        if(_stateMachine != null)
        {
            Gizmos.color = _stateMachine.GetGizmoColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 3, 0.5f);
        }
        else
        {
            Debug.Log("No state machine!");
        }
    }
}
