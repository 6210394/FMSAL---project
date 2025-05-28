using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CookingStation : ScriptableObject
{
    public enum CookingMethod { };

    [System.Serializable]
    public struct MethodRequirements
    {
        public CookingMethod cookingMethod;
        public List<IngredientScript> requiredIngredients;
    }

}


[CustomPropertyDrawer(typeof(CookingStation.MethodRequirements))]
public class CookingProcessDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var methodProp = property.FindPropertyRelative("cookingMethod");
        string methodName = methodProp.enumDisplayNames[methodProp.enumValueIndex];
        EditorGUI.PropertyField(position, property, new GUIContent(methodName), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
