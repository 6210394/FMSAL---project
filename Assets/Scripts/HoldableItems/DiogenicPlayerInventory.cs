using UnityEngine;

public class DiogenicPlayerInventory : MonoBehaviour
{
    public GameObject handAnchor;
    public IObjectType mainHeldObject;
    public IObjectType secondaryHeldItem;


    GameObject instantiatedVisual;

    public static DiogenicPlayerInventory instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Update()
    {
        ShowItemInHands();
    }

    public void ShowItemInHands()
    {
        if (mainHeldObject != null && mainHeldObject.itemMesh != null && instantiatedVisual == null)
        {
            Debug.Log("Creating" + mainHeldObject.name);
            instantiatedVisual = new GameObject("HeldItem");
            instantiatedVisual.transform.SetParent(handAnchor.transform);
            instantiatedVisual.transform.localPosition = -mainHeldObject.grabPoint;
            instantiatedVisual.transform.localRotation = Quaternion.identity;

            MeshFilter meshFilter = instantiatedVisual.AddComponent<MeshFilter>();
            meshFilter.mesh = mainHeldObject.itemMesh;

            MeshRenderer meshRenderer = instantiatedVisual.AddComponent<MeshRenderer>();
            meshRenderer.material = mainHeldObject.itemMaterial;

            instantiatedVisual.transform.localScale *= mainHeldObject.scale;
        }
        
        {
            Destroy(instantiatedVisual);
        }
        
    }

    public void HideItemInHands()
    {
        if(instantiatedVisual != null)
        {
            Destroy(instantiatedVisual);
            instantiatedVisual = null;
        }
    }
}
