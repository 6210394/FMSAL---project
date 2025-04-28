using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof (EnemyCombatController))]
[RequireComponent(typeof (EnemyMovementController))]
public class EnemyBlueprint : MonoBehaviour
{
    public EnemyStats _enemyStats;
    public bool _isReadyToAttack = false; //indicates that the entity is available for attacking to the EnemyManager
    
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

    private Vector3 _lastRaycastDirection;
    private bool _raycastHitPlayer;

    void Awake()
    {
        _combatController = GetComponent<EnemyCombatController>();
        _movementController = GetComponent<EnemyMovementController>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        _stateMachine = new StateMachine();

        _combatController.combatScript.SwitchWeapons(1);
    }

    void Update()
    {
        CheckForPlayersInDetectionRange();
    }

    public void ResetAnimator()
    {
        _animator.SetBool("Strafe", false);
        _animator.SetFloat("Speed", 0);
    }

    protected void OnTakeHit(float stunDuration, Transform damageSource)
    {
        Debug.Log("HAS TAKEN HIT");
        _hasTakenHit = true;
        _damageSource = damageSource.position;
        _stunDuration = stunDuration;
    }

    public void Death()
    {
        _combatController.Die();
        
        ResetAnimator();
        int dieAnimAnex = 0;
        _animator.SetFloat("DeathIndex", dieAnimAnex);
        _animator.SetTrigger("Die"); 

        _stateMachine = null;
        enabled = false;
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
                    _lastRaycastDirection = player.transform.position - transform.position  + new Vector3(0,1,0);

                    if(Physics.Raycast(transform.position + new Vector3(0,1,0), player.transform.position - transform.position, 100))
                    {
                        _target = player.GetComponent<PlayerCombatController>().transform;
                        _combatController.target = _target;

                        if(!_combatController.enemyManager.availableEnemies.Contains(this) && _combatController.isAvailableForEnemyManager)
                        {
                            _combatController.enemyManager.AddEnemy(this);
                        }

                        return true;
                    }                   
                }
            }
            else
            {
                if(_combatController.enemyManager.availableEnemies.Contains(this) || !_combatController.isAvailableForEnemyManager)
                {
                    _combatController.enemyManager.RemoveEnemy(this);
                }
                return false;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if(_stateMachine != null)
        {
            Gizmos.color = _stateMachine.GetGizmoColor();
            Gizmos.DrawSphere(transform.position + Vector3.up * 3, 0.5f);
        }

        if (_combatController == null) return;

        Gizmos.color = _raycastHitPlayer ? Color.green : Color.red;
        if (_lastRaycastDirection != Vector3.zero)
        {
            Gizmos.DrawRay(_combatController.transform.position, _lastRaycastDirection * _combatController.detectionRange);
        }
    }
}
