using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesignPattern;
using System;

public class Inventory : Singleton<Inventory>
{
    public static InventoryItem CarriedItem;

    [SerializeField] InventorySlot[] inventorySlots;
    [SerializeField] InventorySlot[] hotbarSlots;

    // 0=Head, 1=Chest, 2=Legs, 3=Feet
    //[SerializeField] InventorySlot[] equipmentSlots;

    public Transform draggablesTransform;
    [SerializeField] InventoryItem itemPrefab;

    [Header("Item List")]
    [SerializeField] Item[] items;

    //[Header("Debug")]
    //[SerializeField] Button giveItemBtn;

    [Header("UI Management")]
    [SerializeField] private GameObject _inventoryUIRootPanel; // 인벤토리 UI의 최상위 GameObject (Panel 등)

    [Header("Hotbar Management")]
    [SerializeField] private int _currentHotbarSlotIndex = 0; // 현재 선택된 핫바 슬롯 인덱스 (기본값 0)

    [Header("Item Dropping")]
    [SerializeField] private float _dropDistance = 1.5f; // 플레이어로부터 아이템이 떨어질 거리
    [SerializeField] private LayerMask _groundLayer; // 바닥 레이어

    // 핫바 슬롯 변경을 외부에 알리는 이벤트 (UI 업데이트 등에 사용)
    public event Action<int> OnHotbarSlotChanged;


    void Awake()
    {
        SingletonInit();

        // 인벤토리 UI 패널 초기 상태 설정 (시작 시 비활성화)
        if (_inventoryUIRootPanel != null)
        {
            _inventoryUIRootPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Inventory: Inventory UI Root Panel이 할당되지 않았습니다. UI 토글이 작동하지 않을 수 있습니다.");
        }

        // 초기 핫바 슬롯 선택을 알림
        OnHotbarSlotChanged?.Invoke(_currentHotbarSlotIndex);
    }

    void Update()
    {
        if(CarriedItem == null) return;

        CarriedItem.transform.position = Input.mousePosition;
    }

    public void SelectHotbarSlot(int index)
    {
        if (index >= 0 && index < hotbarSlots.Length)
        {
            _currentHotbarSlotIndex = index;
            // 핫바 선택이 변경되었음을 외부에 알립니다.
            OnHotbarSlotChanged?.Invoke(_currentHotbarSlotIndex);
            Debug.Log($"핫바 슬롯 {index + 1}번이 선택되었습니다.");
        }
        else
        {
            Debug.LogWarning($"유효하지 않은 핫바 슬롯 인덱스: {index}. 핫바 슬롯 범위는 0에서 {hotbarSlots.Length - 1}입니다.");
        }
    }

    public Item GetCurrentHotbarItem()
    {
        if (_currentHotbarSlotIndex >= 0 && _currentHotbarSlotIndex < hotbarSlots.Length)
        {
            return hotbarSlots[_currentHotbarSlotIndex].myItemData;
        }
        return null;
    }

    public void ToggleInventoryUI()
    {
        SampleUIManager.Instance.ToggleInventoryUI();
    }


    public void SetCarriedItem(InventoryItem item)
    {
        if(CarriedItem != null)
        {
            if(item.activeSlot.myTag != SlotTag.None && item.activeSlot.myTag != CarriedItem.myItem.itemTag) return;
            item.activeSlot.SetItem(CarriedItem);
        }

        CarriedItem = item;
        CarriedItem.canvasGroup.blocksRaycasts = false;
        item.transform.SetParent(draggablesTransform);
    }


    public void SpawnInventoryItem(Item item = null)
    {
        Item _item = item;
        //if (_item == null)
        ////TODO: 아이템을 얻었을때 어떻게 생성이 될지
        //{ _item = PickRandomItem(); }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            // Check if the slot is empty
            if (inventorySlots[i].myItemUI == null)
            {
                Instantiate(itemPrefab, inventorySlots[i].transform).Initialize(_item, inventorySlots[i]);
                break;
            }
        }


    }
    public void DropItemFromSlot(InventoryItem itemToDropUI)
    {
        if (itemToDropUI == null || itemToDropUI.myItem == null)
        {
            Debug.LogWarning("유효하지 않은 아이템을 버리려고 시도했습니다.");
            CarriedItem = null; // 혹시 모를 상황 대비
            return;
        }

        GameObject itemWorldPrefab = itemToDropUI.myItem.WorldPrefab;
        if (itemWorldPrefab == null)
        {
            Debug.LogWarning($"아이템 '{itemToDropUI.myItem.itemName}'에 연결된 3D 월드 프리팹이 없습니다. 버릴 수 없습니다.", itemToDropUI.myItem);
            // 아이템 프리팹이 없어도 인벤토리에서는 지워야 하므로 아래 ClearSlot() 로직은 진행합니다.
        }
        else
        {
            // 1. 아이템이 떨어질 위치를 계산 (플레이어 전방)
            Transform playerTransform = SamplePlayerManager.Instance.Player.transform; // PlayerController의 transform
            Vector3 dropPosition = playerTransform.position + playerTransform.forward * _dropDistance;
            dropPosition.y += 0.5f;

            RaycastHit hit;
            if (Physics.Raycast(dropPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, _groundLayer))
            {
                dropPosition.y = hit.point.y + 0.1f;
            }

            // 2. 3D 게임 오브젝트를 월드에 인스턴스화
            Instantiate(itemWorldPrefab, dropPosition, Quaternion.identity);
        }

        // 3. 인벤토리 슬롯에서 아이템을 제거하고 UI 인스턴스 파괴
        if (itemToDropUI.activeSlot != null)
        {
            itemToDropUI.activeSlot.ClearSlot(); // 해당 슬롯을 비움 (데이터 및 UI 참조 제거)
            Debug.Log($"아이템 '{itemToDropUI.myItem.itemName}'이(가) 슬롯에서 버려졌습니다.");
        }
        else
        {
            // 슬롯에 할당되지 않은 아이템(예: 드래그 도중 생성된 아이템이 버려진 경우)
            Destroy(itemToDropUI.gameObject);
        }

        // CarriedItem을 비웁니다.
        CarriedItem = null;
    }

    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach(var slot in inventorySlots)
        {
            if(slot.myItemData == item) 
            { 
                count++; 
            }
        }
        foreach(var slot in hotbarSlots)
        {
            if(slot.myItemData == item)
            {
                count++;
            }
        }

        return count;
    }

    public void RemoveItem(Item itemToRemove, int amount = 1)
    {
        int removedCount = 0;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].myItemData == itemToRemove)
            {
                inventorySlots[i].ClearSlot();
                removedCount++;
                if (removedCount == amount) break;
            }
        }

        for(int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].myItemData == itemToRemove)
            {
                hotbarSlots[i].ClearSlot();
                removedCount++;
                if (removedCount >= amount) break;
            }
        }

        Debug.Log($"{itemToRemove.name}{removedCount}개를 인벤토리에서 제거 했습니다.");
    }

    //Item PickRandomItem()
    //{
    //    int random = Random.Range(0, items.Length);
    //    return items[random];
    //}
}
