using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SampleCraftingUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _craftingPanel;
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

    void Awake()
    {
        _craftingPanel.SetActive(false); // 시작 시 제작 UI 비활성화
        _craftingDetailPanel.SetActive(false); // 상세 정보 패널 비활성화

        // 제작 버튼에 클릭 이벤트 리스너 추가
        _craftButton.onClick.AddListener(OnCraftButtonClicked);
    }

    void OnEnable()
    {
        // UI 상태 초기화 및 목록 채우기
        PopulateCraftingList();
        _craftingDetailPanel.SetActive(false);
        _currentSelectedRecipe = null;

        // 이벤트를 한 번만 구독하고, 반드시 널 체크를 포함합니다.
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnRecipeSelected += DisplayRecipeDetails;
            CraftingManager.Instance.OnCraftingCompleted += UpdateUIOnCraftingCompleted;
        }
        else
        {
            Debug.LogWarning("OnEnable: CraftingManager.Instance가 null입니다. 이벤트가 구독되지 않습니다.");
        }

        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnHotbarSlotItemUpdated += OnInventoryOrHotbarChanged;
            // Inventory에 OnInventoryChanged 이벤트가 있다면:
            // Inventory.Instance.OnInventoryChanged += OnInventoryOrHotbarChanged;
        }
        else
        {
            Debug.LogWarning("OnEnable: Inventory.Instance가 null입니다. 이벤트가 구독되지 않습니다.");
        }
    }

    void OnDisable()
    {
        if (CraftingManager.Instance != null) // 명확성을 위한 명시적인 널 체크
        {
            CraftingManager.Instance.OnRecipeSelected -= DisplayRecipeDetails;
            CraftingManager.Instance.OnCraftingCompleted -= UpdateUIOnCraftingCompleted;
        }
        else
        {
            // 애플리케이션 종료/씬 전환 시 유용한 메시지입니다.
            Debug.Log("SampleCraftingUIController.OnDisable 실행 시 CraftingManager.Instance가 이미 파괴되었거나 null입니다. 구독 해제를 건너뜁니다.");
        }
        if (Inventory.Instance != null) // 명확성을 위한 명시적인 널 체크
        {
            Inventory.Instance.OnHotbarSlotItemUpdated -= OnInventoryOrHotbarChanged;
            // 만약 OnInventoryChanged 이벤트를 사용한다면:
            // Inventory.Instance.OnInventoryChanged -= OnInventoryOrHotbarChanged;
        }
        else
        {
            Debug.Log("SampleCraftingUIController.OnDisable 실행 시 Inventory.Instance가 이미 파괴되었거나 null입니다. 구독 해제를 건너뜁니다.");
        }

        // UI가 비활성화될 때 생성된 슬롯들을 정리
        ClearCraftingListSlots();
        ClearMaterialList();
    }
    void Start()
    {
        PopulateCraftingList(); // 시작 시 제작 목록 채우기
    }

    // 제작 UI 패널을 켜고 끄는 함수
    public void ToggleCraftingUI()
    {
        _craftingPanel.SetActive(!_craftingPanel.activeSelf);
        if (_craftingPanel.activeSelf)
        {
            PopulateCraftingList(); // UI가 켜질 때마다 목록 갱신 (선택 사항)
            _craftingDetailPanel.SetActive(false); // UI 켤 때 상세 패널은 비활성화
            _currentSelectedRecipe = null; // 선택된 레시피 초기화
        }
    }

    // 제작 목록을 스크롤뷰에 채우는 함수
    private void PopulateCraftingList()
    {
        ClearCraftingListSlots(); // 기존 슬롯 정리

        // _craftingItemSlotPrefab과 _craftingListContent가 할당되었는지 안전성 체크 추가 
        if (_craftingItemSlotPrefab == null)
        {
            Debug.LogError("_craftingItemSlotPrefab이 할당되지 않았습니다!");
            return;
        }
        if (_craftingListContent == null)
        {
            Debug.LogError("_craftingListContent가 할당되지 않았습니다!");
            return;
        }

        // 모든 레시피를 가져와서 슬롯 생성
        foreach (Recipe recipe in CraftingManager.Instance.AllCraftingRecipes)
        {
            if (recipe.craftedItem == null)
            {
                Debug.LogWarning($"레시피 '{recipe.name}'에 제작 아이템이 할당되지 않았습니다. 이 레시피는 건너뜝니다.");
                continue;
            }

            GameObject slotGO = Instantiate(_craftingItemSlotPrefab, _craftingListContent);
            _instantiatedRecipeSlots.Add(slotGO);

            // --- 여기가 변경되는 부분입니다! ---
            CraftingItemUISlot uiSlot = slotGO.GetComponent<CraftingItemUISlot>();

            if (uiSlot == null)
            {
                Debug.LogError($"'{_craftingItemSlotPrefab.name}' 프리팹에 CraftingItemUISlot 컴포넌트가 없습니다! 확인해주세요.");
                Destroy(slotGO); // 잘못된 슬롯은 파괴
                continue;
            }

            // 이제 uiSlot의 SetUI 메서드를 호출하여 데이터를 설정합니다.
            uiSlot.SetUI(recipe);
        }
    }

    // 선택된 레시피의 상세 정보를 UI에 표시
    private void DisplayRecipeDetails(Recipe recipe)
    {
        _currentSelectedRecipe = recipe;
        _craftingDetailPanel.SetActive(true);

        if (recipe == null)
        {
            // 레시피가 null일 경우 상세 정보 비우기
            _craftedItemImage.sprite = null;
            _craftedItemNameText.text = "아이템 선택 안됨";
            _craftedItemDescriptionText.text = "";
            _craftedItemCurrentAmountText.text = "";
            ClearMaterialList();
            _craftButton.interactable = false;
            return;
        }

        // 제작될 아이템 정보
        if (_craftedItemImage != null) _craftedItemImage.sprite = recipe.craftedItem.icon;
        if (_craftedItemNameText != null) _craftedItemNameText.text = recipe.craftedItem.itemName;
        if (_craftedItemDescriptionText != null) _craftedItemDescriptionText.text = recipe.description;

        // 플레이어가 가진 제작 아이템 개수 표시
        int currentCraftedItemAmount = Inventory.Instance.GetItemCount(recipe.craftedItem);
        if (_craftedItemCurrentAmountText != null) _craftedItemCurrentAmountText.text = $"보유: {currentCraftedItemAmount}개";

        // 재료 목록 표시
        ClearMaterialList(); // 기존 재료 UI 제거
        foreach (var material in recipe.requiredMaterials)
        {
            GameObject materialUI = Instantiate(_materialItemUIPrefab, _materialListContainer);
            if (materialUI == null) continue;
            _instantiatedMaterialUIs.Add(materialUI);

            MaterialItemUISlot uiSlot = materialUI.GetComponent<MaterialItemUISlot>();

            if (uiSlot == null)
            {
                Debug.LogError($"'{_materialItemUIPrefab.name}' 프리팹에 MaterialItemUISlot 컴포넌트가 없습니다! 확인해주세요.");
                continue;
            }

            Sprite icon = material.materialItem?.icon; // Null-conditional operator로 안전하게 접근
            string name = material.materialItem?.itemName;
            int playerHasAmount = Inventory.Instance.GetItemCount(material.materialItem);
            string quantityText = $"{playerHasAmount} / {material.quantity}";
            Color quantityColor = (playerHasAmount < material.quantity) ? Color.red : Color.white;

            uiSlot.SetUI(icon, name, quantityText, quantityColor); // MaterialItemUISlot의 SetUI 메서드 사용
        }

        // 제작 가능 여부에 따라 버튼 활성화/비활성화
        _craftButton.interactable = CraftingManager.Instance.CanCraft(recipe);
    }

    // 재료 목록 UI를 비우는 함수
    private void ClearMaterialList()
    {
        foreach (GameObject go in _instantiatedMaterialUIs)
        {
            Destroy(go);
        }
        _instantiatedMaterialUIs.Clear();
    }

    // "제작하기" 버튼 클릭 시 호출될 함수
    private void OnCraftButtonClicked()
    {
        if (_currentSelectedRecipe != null)
        {
            CraftingManager.Instance.CraftItem(_currentSelectedRecipe);
        }
    }

    // 인벤토리나 핫바 아이템이 변경되었을 때 UI를 갱신
    // (Inventory 스크립트에서 OnInventoryChanged 같은 이벤트가 발생했을 때 호출)
    // 현재는 OnHotbarSlotItemUpdated만 사용하므로 필요에 따라 확장
    private void OnInventoryOrHotbarChanged(int index, Item item, int quantity)
    {
        // 현재 선택된 레시피가 있다면, 해당 레시피의 상세 정보를 다시 표시하여 재료 보유 개수를 갱신
        if (_currentSelectedRecipe != null)
        {
            DisplayRecipeDetails(_currentSelectedRecipe);
        }
    }

    // 제작 완료 후 UI 갱신 (선택된 레시피가 그대로 유지될 경우 사용)
    private void UpdateUIOnCraftingCompleted()
    {
        // 제작이 완료되었으므로, 현재 선택된 레시피의 재료 보유 개수 등을 다시 업데이트
        // DisplayRecipeDetails(_currentSelectedRecipe)가 CraftingManager.OnCraftingCompleted 이벤트에 의해 호출되므로 여기서는 추가 작업 불필요할 수 있음
        // 만약 OnRecipeSelected 이벤트가 발생하지 않는다면 여기서 다시 호출
    }

    private void ClearCraftingListSlots()
    {
        foreach (GameObject go in _instantiatedRecipeSlots)
        {
            Destroy(go);
        }
        _instantiatedRecipeSlots.Clear();
    }


}
