using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public Item craftedItem; // 제작될 아이템 (Item ScriptableObject)
    public int craftedAmount = 1; // 제작될 아이템의 수량

    [System.Serializable]
    public class Material
    {
        public Item materialItem; // 재료 아이템
        public int quantity; // 필요한 수량
    }

    public List<Material> requiredMaterials; // 필요한 재료 목록
    public string description; // 제작 아이템에 대한 설명
}

