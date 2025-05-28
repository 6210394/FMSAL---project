using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Ingredient", menuName = "Cooking/Ingredient Type")]
public class IngredientScript : IObjectType
{
    [Header("Ingredient Details")]
    public float saturationValue;
}
