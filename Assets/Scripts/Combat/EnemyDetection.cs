using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private PlayerCombatController playerCombatController;

    public LayerMask layerMask;
    public float autoLockOnRange = 10f; //THIS NEEDS TO BE CHANGED BASED ON THE WEAPON'S RANGE

    public float sphereCastAOESize = 3f;

    private Vector3 inputDirection;
    [SerializeField] private EnemyCombatController currentTarget;

    void Awake()
    {
        playerCombatController = GetComponent<PlayerCombatController>();
    }

    private void Update()
    {
        if(!playerCombatController.playerMovementController.isFocused)
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
                    ClearTarget();
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

            if(closestTarget != null && closestTarget.GetComponent<EnemyCombatController>().IsAttackable())
            {
                currentTarget = closestTarget.transform.GetComponent<EnemyCombatController>();
                if(Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) <= playerCombatController.combatScript.meleeReach)
                {
                    foreach(SkinnedMeshRenderer meshRenderer in currentTarget.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if(meshRenderer.gameObject.CompareTag("HighlightableMaterial"))
                        {
                            Material material = meshRenderer.material;
                            if (material.HasProperty("_OutlineOpacity"))
                            {
                                material.SetFloat("_OutlineOpacity", 3f);
                            }
                        }
                    }
                }
                
            }
        }
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) > playerCombatController.combatScript.meleeReach)
        {   
            ClearTarget();
        }
    }

    void ClearTarget()
    {
        if(currentTarget != null)
        {
            foreach(SkinnedMeshRenderer meshRenderer in currentTarget.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if(meshRenderer.gameObject.CompareTag("HighlightableMaterial"))
                    {
                        Material material = meshRenderer.material;
                        if (material.HasProperty("_OutlineOpacity"))
                        {
                            material.SetFloat("_OutlineOpacity", 0f);
                        }
                    }
                }
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