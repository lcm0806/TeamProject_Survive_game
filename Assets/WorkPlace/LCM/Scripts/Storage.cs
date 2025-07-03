using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;
using System;

public class Storage : Singleton<Storage>
{
    [SerializeField]
    private InventorySlot[] storageSlots; // 창고 슬롯 배열

    // 필요하다면 창고 UI 패널 등도 관리할 수 있습니다.
    [SerializeField]
    private GameObject _storageUIRootPanel;

    [SerializeField] private InventoryItem _itemPrefab;

    // 창고 아이템 데이터 변경을 알리는 이벤트 (UI 업데이트용)
    public event Action<int, Item, int> OnStorageSlotItemUpdated;

    private void Awake()
    {
        SingletonInit();
        if (_storageUIRootPanel != null)
        {
            _storageUIRootPanel.SetActive(false); // 초기에는 숨김
        }


    }

    public void SetStorageSlots(InventorySlot[] slots)
    {
        this.storageSlots = slots;
        Debug.Log($"Storage: {slots.Length}개의 슬롯이 Storage 인스턴스에 설정되었습니다.");
    }

    public void AddItemToStorage(Item itemData, int quantity)
    {
        if (itemData == null || quantity <= 0) return;

        // itemPrefab이 할당되었는지 확인
        if (_itemPrefab == null)
        {
            Debug.LogError("Storage: InventoryItem Prefab이 할당되지 않았습니다. Inspector를 확인하세요!");
            return;
        }

        if (itemData.isStackable)
        {
            foreach (var slot in storageSlots)
            {
                if (slot.myItemData == itemData && slot.myItemUI != null && slot.myItemUI.CurrentQuantity < itemData.maxStackSize)
                {
                    int spaceLeft = itemData.maxStackSize - slot.myItemUI.CurrentQuantity;
                    int actualAdd = Mathf.Min(quantity, spaceLeft);
                    slot.myItemUI.CurrentQuantity += actualAdd;
                    quantity -= actualAdd;

                    //slot.UpdateSlotUI();
                    OnStorageSlotItemUpdated?.Invoke(Array.IndexOf(storageSlots, slot), itemData, slot.myItemUI.CurrentQuantity); // 이벤트 발생

                    if (quantity <= 0) return; // 모두 추가됨
                }
            }
        }

        while (quantity > 0)
        {
            foreach (var slot in storageSlots)
            {
                if (slot.myItemUI == null) // 빈 슬롯을 찾음
                {
                    InventoryItem newItemUI = Instantiate(_itemPrefab, slot.transform);
                    newItemUI.Initialize(itemData, slot);
                    RectTransform itemRectTransform = newItemUI.GetComponent<RectTransform>();
                    if (itemRectTransform != null)
                    {
                        // 예시: 슬롯 크기에 꽉 채우거나 약간 작게 만듭니다.
                        // Anchor Presets을 이용하는 방식
                        //itemRectTransform.anchorMin = Vector2.zero;   // 왼쪽 아래
                        //itemRectTransform.anchorMax = Vector2.one;    // 오른쪽 위
                        //itemRectTransform.sizeDelta = new Vector2(0, 0); // 부모 크기에 꽉 채움

                        // 또는 고정된 크기로 설정 (부모 크기와 무관)
                        itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 80); // 너비 80
                        itemRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80);   // 높이 80

                        // 약간의 패딩을 주려면 (sizeDelta를 음수로 설정)
                        itemRectTransform.sizeDelta = new Vector2(-10, -10); // 좌우상하 5씩 패딩
                    }
                    slot.SetItem(newItemUI);
                    int addedQuantity = Mathf.Min(quantity, itemData.maxStackSize);
                    newItemUI.CurrentQuantity = addedQuantity;

                    quantity -= addedQuantity;

                    slot.UpdateSlotUI(); // UI 업데이트를 위해 추가
                    OnStorageSlotItemUpdated?.Invoke(Array.IndexOf(storageSlots, slot), itemData, slot.myItemUI.CurrentQuantity); // 이벤트 발생

                    Debug.Log($"창고에 '{itemData.itemName}' {slot.myItemUI.CurrentQuantity}개 추가됨.");
                    break; // 다음 아이템 또는 남은 수량 처리
                }
            }
            if (quantity > 0)
            {
                Debug.LogWarning($"창고가 가득 차서 '{itemData.itemName}' {quantity}개를 모두 추가할 수 없었습니다.");
                break;
            }
        }


    }

    public int GetItemCount(Item itemData)
    {
        if (itemData == null) return 0;

        int count = 0;
        foreach (var slot in storageSlots)
        {
            // InventorySlot이 Item 데이터를 직접 가지고 있는지 확인
            // 또는 InventorySlotUI가 연결되어 있고 해당 UI가 Item 데이터를 가지고 있다면 그렇게 접근
            if (slot.myItemData == itemData)
            {
                if (slot.myItemUI != null)
                {
                    count += slot.myItemUI.CurrentQuantity;
                }
                // 만약 myItemUI가 null이지만 myItemData는 할당되어 있다면
                // (데이터는 있는데 UI가 아직 생성되지 않은 경우)
                // InventorySlot 자체에 수량 정보가 있다면 그 정보를 사용해야 합니다.
                // slot.CurrentQuantity 같은 필드가 InventorySlot에 있다면 더 정확할 수 있습니다.
            }
        }
        return count;
    }
    public void ToggleStorageUI()
    {
        if (_storageUIRootPanel != null)
        {
            _storageUIRootPanel.SetActive(!_storageUIRootPanel.activeSelf);
        }
    }

    public void RemoveItem(Item itemData, int quantityToRemove)
    {
        if (itemData == null || quantityToRemove <= 0) return;

        // 스택 가능한 아이템부터 역순으로 제거 (선택 사항: 전략에 따라)
        // 여기서는 가장 앞에 있는 아이템부터 제거하는 것으로 가정합니다.
        foreach (var slot in storageSlots)
        {
            if (slot.myItemData == itemData && slot.myItemUI != null)
            {
                int currentQuantity = slot.myItemUI.CurrentQuantity;
                int actualRemove = Mathf.Min(quantityToRemove, currentQuantity);

                slot.myItemUI.CurrentQuantity -= actualRemove;
                quantityToRemove -= actualRemove;

                if (slot.myItemUI.CurrentQuantity <= 0)
                {
                    // 슬롯의 아이템 데이터와 UI를 완전히 제거
                    slot.ClearSlot(); // InventorySlot에 ClearSlot() 메서드가 필요합니다!
                }
                else
                {
                    slot.UpdateSlotUI(); // UI 업데이트
                }

                OnStorageSlotItemUpdated?.Invoke(Array.IndexOf(storageSlots, slot), itemData, slot.myItemUI.CurrentQuantity); // 이벤트 발생

                if (quantityToRemove <= 0) break; // 모두 제거됨
            }
        }

        if (quantityToRemove > 0)
        {
            Debug.LogWarning($"창고에서 '{itemData.itemName}' {quantityToRemove}개를 모두 제거할 수 없었습니다. 충분한 수량이 없습니다.");
        }
    }
}
