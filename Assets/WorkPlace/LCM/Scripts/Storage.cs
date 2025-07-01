using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class Storage : Singleton<Storage>
{
    [SerializeField]
    private InventorySlot[] storageSlots; // 창고 슬롯 배열

    // 필요하다면 창고 UI 패널 등도 관리할 수 있습니다.
    [SerializeField]
    private GameObject _storageUIRootPanel;

    private void Awake()
    {
        SingletonInit();
    }

    public void AddItemToStorage(Item itemData, int quantity)
    {
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
                    // InventoryItem 프리팹을 Instanitate하여 창고 슬롯에 할당
                    // (이 부분은 Inventory에서 itemPrefab을 public으로 하거나,
                    // Storage에서도 itemPrefab을 참조하도록 설정해야 합니다.)
                    // 예시: var newItemUI = Instantiate(Inventory.Instance.itemPrefab, slot.transform);
                    // newItemUI.Initialize(itemData, slot);
                    // newItemUI.CurrentQuantity = Mathf.Min(quantity, itemData.maxStackSize);
                    // quantity -= newItemUI.CurrentQuantity;

                    // 간략화를 위해 슬롯 데이터만 설정하고 UI는 나중에 다시 그리는 방식 고려
                    // 실제 구현에서는 InventoryItem UI를 생성하고 초기화하는 로직이 필요합니다.
                    slot.SetItemData(itemData); // 슬롯에 아이템 데이터만 설정하는 새로운 메서드가 필요할 수 있음
                    slot.SetItemQuantity(Mathf.Min(quantity, itemData.maxStackSize)); // 수량 설정
                    quantity -= Mathf.Min(quantity, itemData.maxStackSize);

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
    public void ToggleStorageUI()
    {
        if (_storageUIRootPanel != null)
        {
            _storageUIRootPanel.SetActive(!_storageUIRootPanel.activeSelf);
        }
    }
}
