using UnityEngine;

[RequireComponent(typeof (EnemyCombatController))]
[RequireComponent(typeof (EnemyMovementController))]
public class EnemyBlueprint : MonoBehaviour
{
    public EnemyStats _enemyStats;
    public bool _isReadyToAttack = false; //indicates that the player is available for attacking to the EnemyManager
    
    public bool _hasTakenHit = false;
    public Vector3 _damageSource;
    public float _stunDuration = 0;
    public float _stunChainRecoveryRate;

    public bool wantsToRetreat = false;

    public EnemyCombatController _combatController;
    public EnemyMovementController _movementController;
    public Animator _animator;

    protected StateMachine _stateMachine;

    public Transform _target;

    void Awake()
    {
        _combatController = GetComponent<EnemyCombatController>();
        _movementController = GetComponent<EnemyMovementController>();
        _animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _stateMachine = new StateMachine();

        _combatController.combatScript.SwitchWeapons(1);
    }
    
    public void ResetAnimator()
    {
        _animator.SetBool("Strafe", false);
        _animator.SetFloat("Speed", 0);
    }

    protected void OnTakeHit(float stunDuration, Transform damageSource)
    {
        _hasTakenHit = true;
        _damageSource = damageSource.position;
        _stunDuration = stunDuration;
    }

    public void Death()
    {
        int dieAnimAnex = UnityEngine.Random.Range(1,4);
        _animator.SetFloat("deathIndex", dieAnimAnex);
        _animator.SetTrigger("Die");
        _stateMachine = null;
    }

    protected bool CheckForPlayersInDetectionRange()
    {
        foreach (GameObject player in _combatController.players)
        {
            Vector3 directionToPlayer = player.transform.position - _combatController.transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= _combatController.detectionRange)
            {
                float angleToPlayer = Vector3.Angle(_combatController.transform.forward, directionToPlayer);


                if (angleToPlayer <= _combatController.fieldOfViewAngle / 2)
                {
                    Debug.Log("Player detected");
                    _target = player.GetComponent<PlayerCombatController>().transform;
                    _combatController.target = _target;
                    return true;
                }
            }
            else
            {
                _target = null;
                return false;
            }
        }
        return false;
    }
}
