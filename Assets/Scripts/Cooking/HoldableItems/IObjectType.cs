using UnityEngine;

[CreateAssetMenu(fileName = "HoldableItem", menuName = "Scriptable Objects/HoldableItem")]
public class IObjectType : ScriptableObject
{
    #region Model Details
        public Vector3 grabPoint;
        
        public Mesh itemMesh;
        public Material itemMaterial;

        public float scale = 1;
    #endregion

    #region Object Details
        public bool isHeavy;
        public bool isIngredient;
        public bool isThrowable;

        public int damage;
        public float stunTime;
    #endregion
}
