using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public Item outputItem;
    public int outputAmount = 1;

    [System.Serializable]
    public class CraftingMaterial
    {
        public Item materialItem;
        public int requiredAmount;
    }

    public List<CraftingMaterial> requireMaterials;
}

