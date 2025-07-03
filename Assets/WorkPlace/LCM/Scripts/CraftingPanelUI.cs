using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingPanelUI : MonoBehaviour
{
    public StorageManager storageManager; // StorageManager 스크립트 참조
    public Button openStorageButton;

    [Header("Dependencies")]
    [SerializeField] private CraftingManager _craftingManager; // CraftingManager 참조
    //[SerializeField] private Inventory _inventory;             // Inventory 참조

    [Header("UI References")]
    [SerializeField] private GameObject _craftingPanel; // 전체 제작 UI를 담고 있는 부모 GameObject (이 스크립트가 붙을 GameObject 자체가 패널일 수도 있음)
    [SerializeField] private RectTransform _craftingListContent; // 스크롤뷰의 Content
    [SerializeField] private GameObject _craftingItemSlotPrefab; // 제작 아이템 슬롯 프리팹

    [Header("Detail Panel References")]
    [SerializeField] private GameObject _craftingDetailPanel;
    [SerializeField] private Image _craftedItemImage;
    [SerializeField] private TextMeshProUGUI _craftedItemNameText;
    [SerializeField] private TextMeshProUGUI _craftedItemDescriptionText;
    [SerializeField] private TextMeshProUGUI _craftedItemCurrentAmountText;
    [SerializeField] private Transform _materialListContainer; // 재료 목록이 들어갈 부모
    [SerializeField] private GameObject _materialItemUIPrefab; // 재료 항목 UI 프리팹
    [SerializeField] private Button _craftButton;

    private Recipe _currentSelectedRecipe; // 현재 선택된 레시피

    private List<GameObject> _instantiatedRecipeSlots = new List<GameObject>(); // 생성된 레시피 슬롯 관리
    private List<GameObject> _instantiatedMaterialUIs = new List<GameObject>(); // 생성된 재료 UI 관리



    private void Awake()
    {
        // 기존 CraftingPanelUI의 Awake 내용 (만약 있었다면)
        // Example: UI 시작 시 비활성화
        // _craftingPanel.SetActive(false); // 만약 이 스크립트가 부모 패널이라면 필요
        // _craftingDetailPanel.SetActive(false);

        // SampleCraftingUIController의 Awake 내용 통합
        // 이 스크립트가 CraftingPanel 자체에 붙는다면 _craftingPanel 참조는 this.gameObject로 변경될 수 있습니다.
        // 여기서는 _craftingPanel은 전체 Crafting UI를 의미한다고 가정하고 그대로 둡니다.
        if (_craftingDetailPanel != null) _craftingDetailPanel.SetActive(false);

        // 안전성 체크
        if (_craftingManager == null) { Debug.LogError("CraftingManager가 할당되지 않았습니다!", this); enabled = false; return; }
        if (_craftButton != null) _craftButton.onClick.AddListener(OnCraftButtonClicked);

        // 기존 CraftingPanelUI의 openStorageButton 연결
        if (openStorageButton != null)
        {
            openStorageButton.onClick.AddListener(OpenStorage);
        }
        else
        {
            Debug.LogWarning("openStorageButton이 CraftingPanelUI 스크립트에 할당되지 않았습니다.");
        }


    }

    private void OnEnable()
    {
        // SampleCraftingUIController의 OnEnable 내용 통합
        PopulateCraftingList();
        _craftingDetailPanel.SetActive(false);
        _currentSelectedRecipe = null;

        if (_craftingManager != null)
        {
            _craftingManager.OnRecipeSelected += DisplayRecipeDetails;
            _craftingManager.OnCraftingCompleted += UpdateUIOnCraftingCompleted;
        }
        if (Storage.Instance != null)
        {
            Storage.Instance.OnStorageSlotItemUpdated += OnInventoryOrHotbarChanged;
        }
    }

    private void OnDisable()
    {
        // SampleCraftingUIController의 OnDisable 내용 통합
        if (_craftingManager != null)
        {
            _craftingManager.OnRecipeSelected -= DisplayRecipeDetails;
            _craftingManager.OnCraftingCompleted -= UpdateUIOnCraftingCompleted;
        }
        if (Storage.Instance != null)
        {
            Storage.Instance.OnStorageSlotItemUpdated -= OnInventoryOrHotbarChanged;
        }

        ClearCraftingListSlots();
        ClearMaterialList();
    }
    private void Start()
    {

    }

    public void OpenStorage()
    {
        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.OpenStorageUI();
        }
        else
        {
            Debug.LogError("StorageManager 참조가 CraftingPanelUI에 할당되지 않았습니다!");
        }
    }

    public void CloseStorage()
    {
        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.CloseStorageUI();
        }
        else
        {
            Debug.LogError("StorageManager 참조가 CraftingPanelUI에 할당되지 않았습니다!");
        }
    }

    private void PopulateCraftingList()
    {
        ClearCraftingListSlots();

        if (_craftingItemSlotPrefab == null) { Debug.LogError("_craftingItemSlotPrefab이 할당되지 않았습니다!"); return; }
        if (_craftingListContent == null) { Debug.LogError("_craftingListContent가 할당되지 않았습니다!"); return; }
        if (_craftingManager == null) { Debug.LogError("CraftingManager가 할당되지 않았습니다!"); return; }

        foreach (Recipe recipe in _craftingManager.AllCraftingRecipes)
        {
            if (recipe.craftedItem == null) { Debug.LogWarning($"레시피 '{recipe.name}'에 제작 아이템이 할당되지 않았습니다. 이 레시피는 건너뜝니다."); continue; }

            GameObject slotGO = Instantiate(_craftingItemSlotPrefab, _craftingListContent);
            _instantiatedRecipeSlots.Add(slotGO);

            CraftingItemUISlot uiSlot = slotGO.GetComponent<CraftingItemUISlot>();

            if (uiSlot == null) { Debug.LogError($"'{_craftingItemSlotPrefab.name}' 프리팹에 CraftingItemUISlot 컴포넌트가 없습니다! 확인해주세요."); Destroy(slotGO); continue; }

            uiSlot.SetUI(recipe, _craftingManager);
        }
    }

    private void DisplayRecipeDetails(Recipe recipe)
    {
        _currentSelectedRecipe = recipe;
        _craftingDetailPanel.SetActive(true);

        if (recipe == null)
        {
            _craftedItemImage.sprite = null;
            _craftedItemNameText.text = "아이템 선택 안됨";
            _craftedItemDescriptionText.text = "";
            _craftedItemCurrentAmountText.text = "";
            ClearMaterialList();
            _craftButton.interactable = false;
            return;
        }

        if (_craftedItemImage != null) _craftedItemImage.sprite = recipe.craftedItem.icon;
        if (_craftedItemNameText != null) _craftedItemNameText.text = recipe.craftedItem.itemName;
        if (_craftedItemDescriptionText != null) _craftedItemDescriptionText.text = recipe.description;

        int currentCraftedItemAmount = Storage.Instance.GetItemCount(recipe.craftedItem);
        if (_craftedItemCurrentAmountText != null) _craftedItemCurrentAmountText.text = $"보유: {currentCraftedItemAmount}개";

        ClearMaterialList();
        foreach (var material in recipe.requiredMaterials)
        {
            GameObject materialUI = Instantiate(_materialItemUIPrefab, _materialListContainer);
            if (materialUI == null) continue;
            _instantiatedMaterialUIs.Add(materialUI);

            MaterialItemUISlot uiSlot = materialUI.GetComponent<MaterialItemUISlot>();

            if (uiSlot == null) { Debug.LogError($"'{_materialItemUIPrefab.name}' 프리팹에 MaterialItemUISlot 컴포넌트가 없습니다! 확인해주세요."); continue; }

            Sprite icon = material.materialItem?.icon;
            string name = material.materialItem?.itemName;
            int playerHasAmount = Storage.Instance.GetItemCount(material.materialItem);
            string quantityText = $"{playerHasAmount} / {material.quantity}";
            Color quantityColor = (playerHasAmount < material.quantity) ? Color.red : Color.white;

            uiSlot.SetUI(icon, name, quantityText, quantityColor);
        }

        _craftButton.interactable = _craftingManager.CanCraft(recipe);
    }

    private void ClearMaterialList()
    {
        foreach (GameObject go in _instantiatedMaterialUIs) { Destroy(go); }
        _instantiatedMaterialUIs.Clear();
    }

    private void OnCraftButtonClicked()
    {
        if (_currentSelectedRecipe != null)
        {
            _craftingManager.CraftItem(_currentSelectedRecipe);
        }
    }

    private void OnInventoryOrHotbarChanged(int index, Item item, int quantity)
    {
        if (_currentSelectedRecipe != null)
        {
            DisplayRecipeDetails(_currentSelectedRecipe);
        }
    }

    private void UpdateUIOnCraftingCompleted()
    {
        if (_currentSelectedRecipe != null)
        {
            DisplayRecipeDetails(_currentSelectedRecipe);
        }
        PopulateCraftingList();
    }

    private void ClearCraftingListSlots()
    {
        foreach (GameObject go in _instantiatedRecipeSlots) { Destroy(go); }
        _instantiatedRecipeSlots.Clear();
    }
}
