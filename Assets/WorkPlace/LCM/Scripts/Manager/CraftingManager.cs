using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;
using System;

public class CraftingManager : Singleton<CraftingManager>
{

    [Header("All Crafting Recipes")]
    [SerializeField]
    private List<Recipe> _allCraftingRecipes; // 모든 제작 레시피 목록

    //레시피 목록 접근 프로퍼티
    public List<Recipe> AllCraftingRecipes => _allCraftingRecipes;

    // 선택된 레시피가 변경될 때 호출될 이벤트 (UI 업데이트용)
    public event Action<Recipe> OnRecipeSelected;
    // 제작이 완료되었을 때 호출될 이벤트 (UI 업데이트용)
    public event Action OnCraftingCompleted;


    protected void Awake()
    {
        SingletonInit();
    }
    
    public bool CanCraft(Recipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }

        foreach (var material in recipe.requiredMaterials)
        {
            if (material.materialItem == null)
            {
                Debug.LogWarning($"레시피 '{recipe.name}'에 재료 아이템이 할당되지 않았습니다. 제작 불가.");
                return false;
            }

            int playerHasAmount = Inventory.Instance.GetItemCount(material.materialItem);
            if (playerHasAmount < material.quantity)
            {
                // 디버그는 자주 뜨므로, 필요할 때만 출력하도록 주석 처리하거나 조건부로 사용
                // Debug.Log($"재료 부족: {material.materialItem.name} (보유: {playerHasAmount} / 필요: {material.quantity})");
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
        //재료 소모
        foreach(var material in recipe.requiredMaterials)
        {
            Inventory.Instance.RemoveItem(material.materialItem, material.quantity);
        }
        //아이템 제작 및 인벤토리 추가
        for(int i = 0; i < recipe.craftedAmount; i++)
        {
            Inventory.Instance.SpawnInventoryItem(recipe.craftedItem);
        }

        Debug.Log($"'{recipe.craftedItem.name}' (x{recipe.craftedAmount}) 제작 완료!");
        OnCraftingCompleted?.Invoke(); // 제작 완료 이벤트 발생
        OnRecipeSelected?.Invoke(recipe); // 제작 후 재료 갱신을 위해 선택된 레시피 정보 다시 전달 (선택 상태 유지)
    }

    public void SelectRecipe(Recipe recipe)
    {
        OnRecipeSelected?.Invoke(recipe);
    }

}
