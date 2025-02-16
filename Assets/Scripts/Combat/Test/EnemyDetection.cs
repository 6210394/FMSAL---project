using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    private CombatScript combatScript;

    public LayerMask layerMask;
    public float autoLockOnRange = 10f; //THIS NEEDS TO BE CHANGED BASED ON THE WEAPON'S RANGE

    [SerializeField] Vector3 inputDirection;
    [SerializeField] private EnemyScript currentTarget;

    public GameObject cam;

    private void Start()
    {
        combatScript = GetComponentInParent<CombatScript>();
    }

    private void Update()
    {
        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        inputDirection = forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal");
        inputDirection = inputDirection.normalized;

        RaycastHit info;

        if (Physics.SphereCast(transform.position, 3f, inputDirection, out info, autoLockOnRange, layerMask))
        {
            if(info.collider.transform.GetComponent<EnemyScript>().IsAttackable())
            currentTarget = info.collider.transform.GetComponent<EnemyScript>();
        }
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > autoLockOnRange)
        {
            currentTarget = null;
        }
    }

    public EnemyScript CurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(EnemyScript target)
    {
        currentTarget = target;
    }

    public float InputMagnitude()
    {
        return inputDirection.magnitude;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, inputDirection);
        Gizmos.DrawWireSphere(transform.position, 1);
        if(CurrentTarget() != null)
        Gizmos.DrawSphere(CurrentTarget().transform.position, .5f);
    }
}