using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;
using System;
using TMPro;

public class SampleUIManager : Singleton<SampleUIManager>
{
    
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;

    [Header("Hotbar UI")]
    [SerializeField] private GameObject[] hotbarSelectionIndicators;

    [Header("Crafting UI")]
    [SerializeField] private GameObject craftingPanel; // 제작 UI의 최상위 패널

    [SerializeField] private GameObject sceneSpecificUIRoot;

    public GameObject inventoryPanel;

    private int _currentSelectedHotbarIndex = -1;

    public event Action<bool> OnInventoryUIToggled;

    public event Action<bool> OnCraftingUIToggled;

    

    private void Awake()
    {
        SingletonInit();
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // 인벤토리 UI는 시작 시 비활성화
        }
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
        }

        SetItemDescription(""); // 아이템 설명 초기화

        // Inventory의 핫바 관련 이벤트 구독
        Inventory.Instance.OnHotbarSlotChanged += OnHotbarSlotSelectionChanged;
        Inventory.Instance.OnHotbarSlotItemUpdated += OnHotbarSlotItemContentUpdated;

        if (sceneSpecificUIRoot != null)
        {
            sceneSpecificUIRoot.SetActive(false);
            Debug.Log($"씬 고유 UI 루트 '{sceneSpecificUIRoot.name}'를 비활성화했습니다.");
        }

        // 초기 핫바 선택 상태 UI 업데이트 (Awake에서 Inventory가 초기화된 후 호출)
        // Inventory에서 _currentHotbarSlotIndex를 초기화하므로, 그 값을 반영해야 함
        UpdateHotbarSelectionUI(Inventory.Instance._currentHotbarSlotIndex);
    }
    public void ToggleInventoryUI()
    {
        bool currentStatus = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(currentStatus);

        OnInventoryUIToggled?.Invoke(currentStatus);

        // 커서 및 플레이어 제어 로직
        if (currentStatus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // PlayerController.Instance.SetCanMove(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // PlayerController.Instance.SetCanMove(true);
        }
    }

    public void ToggleCraftingUI()
    {
        bool currentStatus = !craftingPanel.activeSelf;
        craftingPanel.SetActive(currentStatus);

        OnCraftingUIToggled?.Invoke(currentStatus);

        // 인벤토리와 마찬가지로 커서 및 플레이어 제어 로직을 적용
        if (currentStatus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // PlayerController.Instance.SetCanMove(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // PlayerController.Instance.SetCanMove(true);
        }

        // 만약 인벤토리와 제작 UI가 동시에 열릴 수 없다면,
        // 제작 UI를 열 때 인벤토리 UI를 닫고, 그 반대도 마찬가지로 처리해야 합니다.
        if (currentStatus && inventoryPanel.activeSelf)
        {
            ToggleInventoryUI(); // 제작 UI 열리면 인벤토리 닫기
        }
        // 또는, Inventory.cs의 ToggleInventoryUI에서도 이 로직을 추가하여
        // 인벤토리 열릴 때 제작 UI 닫기
    }

    public void SetItemDescription(string description)
    {
        if (_itemDescriptionText != null)
        {
            _itemDescriptionText.text = description;
        }
    }

    private void OnHotbarSlotSelectionChanged(int newIndex)
    {
        _currentSelectedHotbarIndex = newIndex;
        UpdateHotbarSelectionUI(newIndex);
    }

    private void UpdateHotbarSelectionUI(int selectedIndex)
    {
        if (hotbarSelectionIndicators == null || hotbarSelectionIndicators.Length == 0) return;

        for (int i = 0; i < hotbarSelectionIndicators.Length; i++)
        {
            if (hotbarSelectionIndicators[i] != null)
            {
                hotbarSelectionIndicators[i].SetActive(i == selectedIndex);
            }
        }
    }

    // Inventory.OnHotbarSlotItemUpdated 이벤트 핸들러
    // 핫바 슬롯의 아이템 내용이 변경될 때 호출됩니다.
    // 여기서는 주로 아이템 선택 인디케이터가 아닌, 핫바 자체의 아이템 아이콘/수량 업데이트를 담당합니다.
    // 하지만 현재 Inventory.SyncHotbarSlotUI에서 이미 UI를 직접 업데이트하므로,
    // 이 이벤트는 다른 UI 요소(예: 외부 장비 UI)를 동기화하는 데 유용할 수 있습니다.
    private void OnHotbarSlotItemContentUpdated(int index, Item newItemData, int newQuantity)
    {
        // 이 이벤트는 주로 핫바 아이템의 시각적 변화가 일어났을 때 추가적인 UI를 업데이트하는 데 사용됩니다.
        // 예: 선택된 핫바 슬롯의 아이템 아이콘을 다시 로드하거나, 수량을 업데이트하는 등의 로직.
        // 현재는 Inventory.SyncHotbarSlotUI가 직접 UI를 생성/파괴/초기화하므로,
        // 이곳에서는 특별히 추가할 로직이 없을 수 있습니다.
        // 하지만 만약 핫바 슬롯 외부에 해당 아이템의 정보(예: 사용 가능한 스킬 아이콘)를 표시한다면 유용합니다.
        Debug.Log($"핫바 슬롯 {index}의 아이템 내용이 변경되었습니다: {newItemData?.itemName} (수량: {newQuantity})");
    }
}
