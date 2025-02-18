using UnityEngine;

public class DiogenicPlayerInventory : MonoBehaviour
{
    public GameObject handAnchor;
    public IObjectType heldObject;

    GameObject instantiatedVisual;

    void Update()
    {
        ShowItemInHands();
    }

    public void ShowItemInHands()
    {
        if (heldObject != null && heldObject.itemMesh != null && instantiatedVisual == null)
        {
            instantiatedVisual = new GameObject("HeldItem");
            instantiatedVisual.transform.SetParent(handAnchor.transform);
            instantiatedVisual.transform.localPosition = -heldObject.grabPoint;
            instantiatedVisual.transform.localRotation = Quaternion.identity;

            MeshFilter meshFilter = instantiatedVisual.AddComponent<MeshFilter>();
            meshFilter.mesh = heldObject.itemMesh;

            MeshRenderer meshRenderer = instantiatedVisual.AddComponent<MeshRenderer>();
            meshRenderer.material = heldObject.itemMaterial;

            instantiatedVisual.transform.localScale *= heldObject.scale;
        }
        
        /* ONLY USE THIS FOR ADJUSTING
        else
        {
            Destroy(instantiatedVisual);
        }
        */
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
