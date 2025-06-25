using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Crafted Item")]
    public Item craftedItem; // 제작 결과물 아이템 (완성된 퀘스트 아이템 등)
    public int craftedAmount = 1; // 제작 시 얻는 개수

    [Header("Required Materials")]
    // 필요한 재료 아이템과 각 재료의 수량
    public List<CraftingMaterial> requiredMaterials = new List<CraftingMaterial>();

    [System.Serializable]
    public class CraftingMaterial
    {
        public Item materialItem;
        public int requiredAmount;
    }
}

