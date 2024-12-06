using UnityEngine;
using UnityEngine.SceneManagement;

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
        GameManager.onPlayersListed.AddListener(Initialize);
        interactCollider = GetComponent<SphereCollider>();
        interactCollider.isTrigger = true;
        interactCollider.radius = interactRadius;
        icon = GetComponentInChildren<FloatingIcons>();
        icon.SetIconActive(false);
        Initialize();
    }

    void Initialize()
    {
        if(SceneManager.GetActiveScene().name == "Home")
        {
            Debug.Log("Initializing");
            player = GameManager.instance.players[0];
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
        directionToInteractable.y = 0; // Ignore vertical component

        Vector3 playerForward = playerMovement.cameraTransform.forward;
        playerForward.y = 0; // Ignore vertical component

        float angle = Vector3.Angle(playerForward, directionToInteractable);

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
