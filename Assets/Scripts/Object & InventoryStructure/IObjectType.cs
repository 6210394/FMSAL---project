using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "Objects/Simple Object Type")]
public class IObjectType : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStackSize = 1;

    [Header("Item Properties")]
    public int value;
    public bool isConsumable = false;

    [Header("Model Info")]
    public Vector3 grabPoint;

    public Mesh itemMesh;
    public Material itemMaterial;

    public float scale = 1;
    public Vector3 rotation;

    public void Use()
    {
        if (!isConsumable)
        {
            return;
        }
    }
}
