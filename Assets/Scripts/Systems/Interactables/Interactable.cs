using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class Interactable : MonoBehaviour
{   
    protected bool highlighted = false;
    protected List<Material> materials = new List<Material>();

    public float interactRadius = 3f;
    public float interactionAngle = 360;
    public SphereCollider interactCollider;

    public FloatingIcons icon;

    public GameObject player;
    public KeyCode interactKey = KeyCode.E;


    void Start()
    {
        icon = GetComponentInChildren<FloatingIcons>();
        interactCollider = GetComponent<SphereCollider>();


        if(interactCollider != null)
        {
            interactCollider.isTrigger = true;
            interactCollider.radius = interactRadius;
        }

        if(icon != null)
        {
            icon.SetIconActive(false);
        }
        else
        {
            Debug.LogWarning(this + " is an interactable object but doesnt have an interact icon!!");
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player = other.gameObject;
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player = null;
            icon.SetIconActive(false);
        }
    }

    public virtual void Update()
    {
        PlayerInRangeCheck();
    }
    

    public virtual void Interact()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    void PlayerInRangeCheck()
    {
        if(!GameManager.instance.interactEnabled)
        {
            return;
        }

        if(player != null && Vector3.Distance(player.transform.position, transform.position) <= interactRadius)
        {   
            PlayerMovementController playerMovement = player.GetComponent<PlayerMovementController>();

            Vector3 directionToInteractable = (transform.position - player.transform.position).normalized;
            directionToInteractable.y = 0; // Ignore vertical component

            Vector3 playerForward = playerMovement.cameraTransform.forward;
            playerForward.y = 0; // Ignore vertical component

            float angle = Vector3.Angle(playerForward, directionToInteractable);

            if (angle <= 45f)
            {
                if(icon)
                {
                    icon.SetIconActive(true);
                }
                if(Input.GetKeyDown(interactKey))
                {
                    Interact();
                }
            }
            else
            {   
                if(icon)
                {
                    icon.SetIconActive(false);
                }
            }
        }
    }
}
