using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SampleCraftingUIController : MonoBehaviour
{
    [SerializeField] private Button craftButton;
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private TextMeshProUGUI materialInfoText;

    public Recipe currentSelectedRecipe;
    private void Start()
    {
        craftButton.onClick.AddListener(OnCraftButtonClicked);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (currentSelectedRecipe != null)
        {
            recipeNameText.text = currentSelectedRecipe.craftedItem.itemName;
            // 재료 정보 표시 로직
            string materialsString = "재료:\n";
            bool canCraft = true;
            foreach (var material in currentSelectedRecipe.requiredMaterials)
            {
                int playerHas = Inventory.Instance.GetItemCount(material.materialItem);
                materialsString += $"{material.materialItem.name}: {playerHas}/{material.quantity}\n";
                if (playerHas < material.quantity) canCraft = false;
            }
            materialInfoText.text = materialsString;

            // 제작 가능 여부에 따라 버튼 활성화/비활성화
            craftButton.interactable = CraftingManager.Instance.CanCraft(currentSelectedRecipe); // <- CraftingManager 사용!
        }
        else
        {
            recipeNameText.text = "레시피 선택 안됨";
            materialInfoText.text = "";
            craftButton.interactable = false;
        }
    }

    void OnCraftButtonClicked()
    {
        if (currentSelectedRecipe != null)
        {
            // CraftingManager를 통해 아이템 제작 요청
            CraftingManager.Instance.CraftItem(currentSelectedRecipe); // <- CraftingManager 사용!
            RefreshUI(); // 제작 후 UI 갱신
        }
    }

    public void SetSelectedRecipe(Recipe newRecipe)
    {
        currentSelectedRecipe = newRecipe;
        RefreshUI();
    }
}
