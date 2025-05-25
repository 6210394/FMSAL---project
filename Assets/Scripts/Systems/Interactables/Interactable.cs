using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class Interactable : MonoBehaviour
{   
    protected bool highlighted = false;
    public List<Material> materials = new List<Material>();

    public float interactRadius = 3f;
    public float interactionAngle = 360;
    public SphereCollider interactCollider;

    public FloatingIcons icon;

    public GameObject player;

    public bool interactable = false;
    public KeyCode interactKey = KeyCode.E;

    void Start()
    {
        CollectMaterials();

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
    }

    public virtual void OnTriggerEnter(Collider other)
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
            if(icon)
            {
                icon.SetIconActive(false);
            }
        }
    }

    public virtual void Update()
    {
        PlayerInRangeCheck();
        if(Input.GetKeyDown(interactKey) && interactable)
        {
            Interact();
        }
    }
    

    public virtual void Interact()
    {
        Debug.Log("Interacting with " + gameObject.name);
    }

    void CollectMaterials()
    {
        Transform currentParent = transform.parent;
        if (currentParent == null)
        {
            Debug.LogWarning("No Parent found for " + gameObject.name + ". Please separate the interaction zone from the visuals.");
            return;
        }

        MeshRenderer[] parentedRenderers = currentParent.GetComponentsInChildren<MeshRenderer>();
        if (parentedRenderers.Length > 0)
        {
            foreach (MeshRenderer renderer in parentedRenderers)
            {
                materials.Add(renderer.material);
            }
        }
    }

    private void MarkOutline()
    {
        if (!highlighted)
        {
            highlighted = true;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_OutlineOpacity"))
                {
                    material.SetFloat("_OutlineOpacity", 3f);
                }
                Debug.Log(material.name + " should be highlighted!");
            }
        }
    }

    private void ClearOutline()
    {
        if (highlighted)
        {
            highlighted = false;

            foreach (Material material in materials)
            {
                if (material.HasProperty("_OutlineOpacity"))
                {
                    material.SetFloat("_OutlineOpacity", 0f); 
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    protected void PlayerInRangeCheck()
    {
        if(!GameManager.instance.interactEnabled)
        {
            Debug.Log("Interact is disabled");
            return;
        }

        if(player != null && Vector3.Distance(player.transform.position, transform.position) <= interactRadius)
        { 
            Debug.Log("Player is in range of " + gameObject.name);
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
                interactable = true;

                if(!highlighted)
                {
                    MarkOutline();
                }
                
            }

            else
            {   
                if(icon)
                {
                    icon.SetIconActive(false);
                }

                if(interactable)
                {
                    interactable = false;

                    if(highlighted)
                    {
                        ClearOutline();
                    }
                }                
            }
        }        
    }
}
