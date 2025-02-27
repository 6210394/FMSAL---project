using UnityEngine;

public class DiogenicPlayerInventory : MonoBehaviour
{
    [Header("Booleans")]
    public bool canDropItem;

    [Header("Hand Anchor Reference")]
    public GameObject handAnchor;

    [Header("Hand Slots")]
    public IObjectType mainHeldObject;
    public IObjectType secondaryHeldItem;
    [Space]
    public WeaponScript mainWeapon;
    public WeaponScript sidearm;


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

    void RecieveObject(WeaponScript weapon)
    {
        if(mainWeapon = null)
        {
            mainWeapon = weapon;
        }
        else if (sidearm = null)
        {
            sidearm = weapon;
        }
        ShowItemInHands(weapon);
    }

    void RecieveObject(IObjectType objectType)
    {
        mainHeldObject = objectType;
        ShowItemInHands(objectType);
    }

    public void ShowItemInHands(IObjectType objectType)
    {
        HideItemInHands();

        Debug.Log("Creating" + objectType.name);
        instantiatedVisual = new GameObject("HeldItem");
        instantiatedVisual.transform.SetParent(handAnchor.transform);
        instantiatedVisual.transform.localPosition = -objectType.grabPoint;
        instantiatedVisual.transform.localRotation = Quaternion.Euler(objectType.rotation);

        MeshFilter meshFilter = instantiatedVisual.AddComponent<MeshFilter>();
        meshFilter.mesh = objectType.itemMesh;

        MeshRenderer meshRenderer = instantiatedVisual.AddComponent<MeshRenderer>();
        meshRenderer.material = objectType.itemMaterial;

        instantiatedVisual.transform.localScale *= objectType.scale;
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
