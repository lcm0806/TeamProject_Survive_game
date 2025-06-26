using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class CraftingManager : Singleton<CraftingManager>
{

    [Header("All Crafting Recipes")]
    [SerializeField]
    private List<Recipe> _allCraftingRecipes; // 모든 제작 레시피 목록

    //레시피 목록 접근 프로퍼티
    public List<Recipe> AllCraftingRecipes => _allCraftingRecipes;


    protected void Awake()
    {
        SingletonInit();
    }
    
    public bool CanCraft(Recipe recipe)
    {
        if(recipe == null)
        {
            return false;
        }

        foreach(var material in recipe.requiredMaterials)
        {
            if(material.materialItem == null)
            {
                Debug.LogWarning($"레시피에 재료 아이템이 할당 안됨");
                return false;
            }

            int playerHasAmount = Inventory.Instance.GetItemCount(material.materialItem);
            if (playerHasAmount < material.quantity)
            {
                Debug.Log($"재료 부족: {material.materialItem.name} ({playerHasAmount}/{material.quantity}) for {recipe.craftedItem.name}");
                return false;
            }
        }

        return true;
    }

    public void CraftItem(Recipe recipe)
    {

        if (!CanCraft(recipe))
        {
            Debug.LogWarning($"레시피 '{recipe.name}'을 제작 할수 없습니다. 재료를 확인해 주세요");
            return;
        }

        foreach(var material in recipe.requiredMaterials)
        {
            Inventory.Instance.RemoveItem(material.materialItem, material.quantity);
        }

        for(int i = 0; i < recipe.craftedAmount; i++)
        {
            Inventory.Instance.SpawnInventoryItem(recipe.craftedItem);
        }

        Debug.Log("제작완료");
    }
}
