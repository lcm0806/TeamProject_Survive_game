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
        // OnEnable에서는 항상 구독합니다.
        // 하지만 혹시 모를 상황을 대비해 널 체크를 할 수도 있습니다 (정상 플레이 시에는 널이 아니어야 함)
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnRecipeSelected += DisplayRecipeDetails;
            CraftingManager.Instance.OnCraftingCompleted += UpdateUIOnCraftingCompleted;
        }
        else
        {
            Debug.LogWarning("OnEnable에서 CraftingManager.Instance가 null입니다. 정상적인 플레이 중에는 이러면 안 됩니다.");
        }
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnHotbarSlotItemUpdated += OnInventoryOrHotbarChanged;
            // 만약 OnInventoryChanged 이벤트를 사용한다면:
            // Inventory.Instance.OnInventoryChanged += OnInventoryOrHotbarChanged;
        }
        else
        {
            Debug.LogWarning("OnEnable에서 Inventory.Instance가 null입니다. 정상적인 플레이 중에는 이러면 안 됩니다.");
        }
        PopulateCraftingList(); // UI가 켜질 때마다 목록 갱신
        _craftingDetailPanel.SetActive(false); // 상세 패널은 비활성화
        _currentSelectedRecipe = null; // 선택된 레시피 초기화

        // CraftingManager와 Inventory 이벤트 구독
        CraftingManager.Instance.OnRecipeSelected += DisplayRecipeDetails;
        CraftingManager.Instance.OnCraftingCompleted += UpdateUIOnCraftingCompleted;
        Inventory.Instance.OnHotbarSlotItemUpdated += OnInventoryOrHotbarChanged;
        // Inventory.Instance.OnInventoryChanged += OnInventoryOrHotbarChanged; // Inventory에 이 이벤트가 있다면 사용
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
        // 기존 슬롯 제거
        foreach (GameObject go in _instantiatedRecipeSlots)
        {
            Destroy(go);
        }
        _instantiatedRecipeSlots.Clear();

        // 모든 레시피를 가져와서 슬롯 생성
        foreach (Recipe recipe in CraftingManager.Instance.AllCraftingRecipes)
        {
            // 레시피의 제작 아이템이 할당되었는지 확인
            if (recipe.craftedItem == null)
            {
                Debug.LogWarning($"레시피 '{recipe.name}'에 제작 아이템이 할당되지 않았습니다. 이 레시피는 건너뜝니다.");
                continue; // 다음 레시피로 넘어감
            }

            GameObject slotGO = Instantiate(_craftingItemSlotPrefab, _craftingListContent);
            // 프리팹 인스턴스화가 성공했는지 확인
            if (slotGO == null)
            {
                Debug.LogError($"레시피 '{recipe.name}'의 제작 슬롯을 인스턴스화하지 못했습니다. '_craftingItemSlotPrefab' 또는 '_craftingListContent' 할당을 확인하세요.");
                continue;
            }
            _instantiatedRecipeSlots.Add(slotGO);

            // 슬롯 UI 설정
            Image itemIcon = slotGO.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI itemNameText = slotGO.transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();

            if (itemIcon != null) itemIcon.sprite = recipe.craftedItem.icon;
            if (itemNameText != null) itemNameText.text = recipe.craftedItem.itemName;

            // 아이콘과 텍스트 컴포넌트가 존재하고, 아이템 아이콘 스프라이트가 할당되었는지 확인
            if (itemIcon != null && recipe.craftedItem.icon != null)
            {
                itemIcon.sprite = recipe.craftedItem.icon;
            }
            else
            {
                Debug.LogWarning($"레시피 '{recipe.name}'의 아이콘을 찾을 수 없거나, 아이템 '{recipe.craftedItem.name}'에 스프라이트가 할당되지 않았습니다. (itemIcon 존재: {itemIcon != null}, 아이템 스프라이트 존재: {recipe.craftedItem.icon != null})");
                // 필요하다면 기본 아이콘을 할당하거나 시각적으로 처리
            }

            if (itemNameText != null)
            {
                itemNameText.text = recipe.craftedItem.itemName;
            }
            else
            {
                Debug.LogWarning($"레시피 '{recipe.name}'의 아이템 이름 텍스트 컴포넌트를 찾을 수 없습니다.");
            }

            // 버튼 클릭 이벤트 추가
            Button slotButton = slotGO.GetComponent<Button>();
            if (slotButton != null)
            {
                // 람다 표현식으로 현재 레시피를 인자로 전달
                slotButton.onClick.AddListener(() => CraftingManager.Instance.SelectRecipe(recipe));
            }
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
            _instantiatedMaterialUIs.Add(materialUI);

            Image materialIcon = materialUI.transform.Find("MaterialIcon").GetComponent<Image>();
            TextMeshProUGUI materialName = materialUI.transform.Find("MaterialNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI materialQuantity = materialUI.transform.Find("MaterialQuantityText").GetComponent<TextMeshProUGUI>();

            if (materialIcon != null) materialIcon.sprite = material.materialItem.icon;
            if (materialName != null) materialName.text = material.materialItem.itemName;

            int playerHasAmount = Inventory.Instance.GetItemCount(material.materialItem);
            if (materialQuantity != null)
            {
                materialQuantity.text = $"{playerHasAmount} / {material.quantity}";
                // 필요 개수보다 적으면 빨간색으로 표시
                materialQuantity.color = (playerHasAmount < material.quantity) ? Color.red : Color.white;
            }
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
