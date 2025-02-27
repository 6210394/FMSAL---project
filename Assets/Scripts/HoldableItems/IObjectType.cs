using UnityEngine;

[CreateAssetMenu(fileName = "HoldableItem", menuName = "Scriptable Objects/HoldableItem")]
public class IObjectType : ScriptableObject
{
    [Header("Model Details")]
        public Vector3 grabPoint;
        
        public Mesh itemMesh;
        public Material itemMaterial;

        public float scale = 1;
        public Vector3 rotation; 

}
