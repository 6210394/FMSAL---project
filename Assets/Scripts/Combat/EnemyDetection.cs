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
    [SerializeField] private EnemyCombatController currentTarget;

    public GameObject cam;

    private void Start()
    {
        playerCombatController = GetComponent<PlayerCombatController>();
    }

    private void Update()
    {
        if(!playerCombatController.isLockOnToggle)
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

            if(inputDirection == Vector3.zero)
            {
                inputDirection = transform.forward;
            }

            TargetLock(inputDirection);

            if (currentTarget != null )
            {
                Vector3 toTarget = (currentTarget.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(inputDirection, toTarget);
                if (angle > 80f)
                {
                    currentTarget = null;
                }
            }
        }
    }

    public void TargetLock(Vector3 inputDirection)
    {
        GameObject closestTarget = null;
        RaycastHit[] info = Physics.SphereCastAll(transform.position, sphereCastAOESize, inputDirection, autoLockOnRange, layerMask);

        if (info.Length > 0)
        {
            foreach(RaycastHit hit in info)
            {
                if(hit.collider.gameObject.GetComponent<EnemyCombatController>())
                {
                    if(!closestTarget)
                    {
                        closestTarget = hit.collider.gameObject;
                    }
                    else
                    {
                        if(Vector3.Distance(transform.position, closestTarget.gameObject.transform.position) > Vector3.Distance(transform.position, hit.collider.gameObject.transform.position))
                        {
                            closestTarget = hit.collider.gameObject;
                        }
                    }
                }
            }

            if(closestTarget.GetComponent<EnemyCombatController>().IsAttackable())
            currentTarget = closestTarget.transform.GetComponent<EnemyCombatController>();
        }
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > autoLockOnRange*1.5f)
        {   
            currentTarget = null;
        }
    }

    public EnemyCombatController CurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(EnemyCombatController target)
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