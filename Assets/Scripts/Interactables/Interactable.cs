using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Interactable : MonoBehaviour
{
    //THIS SCRIPT SHOULD BE ATTACHED TO AN EMPTY OBJECT PARENTED TO THE INTERACTABLE OBJECT
    
    public float interactRadius = 3f;
    public SphereCollider interactCollider;

    public FloatingIcons icon;

    public GameObject player;
    public KeyCode interactKey = KeyCode.E;

    void Start()
    {
        interactCollider = GetComponent<SphereCollider>();
        interactCollider.isTrigger = true;
        interactCollider.radius = interactRadius;
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

    void Update()
    {
        PlayerInRangeCheck();
    }
    

    public virtual void Interact()
    {
        Debug.Log("Interacting with " + transform.name);
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
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            Vector3 directionToInteractable = (transform.position - player.transform.position).normalized;
            float angle = Vector3.Angle(playerMovement.cameraTransform.forward, directionToInteractable);

            if (angle <= 45f)
            {
                Debug.Log("Player is looking");
                icon.SetIconActive(true);
                if(Input.GetKeyDown(interactKey))
                {
                    Interact();
                }
            }
            else
            {
                icon.SetIconActive(false);
            }
        }
    }
}
