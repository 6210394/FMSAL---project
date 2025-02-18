using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;

    public PlayerCombatController playerCombatController;

    public LayerMask layerMask;
    public float autoLockOnRange = 10f; //THIS NEEDS TO BE CHANGED BASED ON THE WEAPON'S RANGE

    public float sphereCastAOESize = 3f;

    [SerializeField] Vector3 inputDirection;
    [SerializeField] private EnemyScript currentTarget;

    public GameObject cam;

    private void Start()
    {
        playerCombatController = GetComponent<PlayerCombatController>();
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

        if (Physics.SphereCast(transform.position, sphereCastAOESize, inputDirection, out info, autoLockOnRange, layerMask))
        {
            if(info.collider.transform.GetComponent<EnemyScript>().IsAttackable())
            currentTarget = info.collider.transform.GetComponent<EnemyScript>();
        }
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > autoLockOnRange*1.5f)
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

        // Draw the SphereCast
        Gizmos.color = Color.red;
        Vector3 endPosition = transform.position + inputDirection * autoLockOnRange;
        Gizmos.DrawWireSphere(transform.position, sphereCastAOESize);
        Gizmos.DrawWireSphere(endPosition, sphereCastAOESize);

        if (CurrentTarget() != null)
        {
            Gizmos.DrawSphere(CurrentTarget().transform.position, .5f);
        }
    }
}