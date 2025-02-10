using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private EnemyManager enemyManager;
    private CombatScript combatScript;

    public LayerMask layerMask;

    [SerializeField] Vector3 inputDirection;
    [SerializeField] private EnemyScript currentTarget;

    public GameObject cam;

    public bool isLockedOn = false;

    private void Start()
    {
        combatScript = GetComponentInParent<CombatScript>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LockOnAndOff();
        }

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

        if (isLockedOn) 
        {
            if (Physics.SphereCast(transform.position, 3f, inputDirection, out info, 10,layerMask))
            {
                if(info.collider.transform.GetComponent<EnemyScript>().IsAttackable())
                currentTarget = info.collider.transform.GetComponent<EnemyScript>();
            }
        }
        else 
        {
            currentTarget = null;
        }

    }


    public void LockOnAndOff()
    {
        isLockedOn = !isLockedOn; 
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