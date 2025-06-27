using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DesignPattern;
using System;
using TMPro;

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
    [SerializeField] private float _scatterForce = 2f;

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
            item.activeSlot.SetItem(CarriedItem);
        }

        CarriedItem = item;
        CarriedItem.canvasGroup.blocksRaycasts = false;
        item.transform.SetParent(draggablesTransform);
    }


    public void SpawnInventoryItem(Item item)
    {
        // 스택 가능한 아이템인 경우, 먼저 기존 슬롯을 확인
        if (item.isStackable)
        {
            foreach (var slot in hotbarSlots) // 또는 inventorySlots
            {
                if (slot.myItemData == item && slot.myItemUI != null && slot.myItemUI.CurrentQuantity < item.maxStackSize)
                {
                    slot.myItemUI.CurrentQuantity++;
                    Debug.Log($"SUCCESS: '{item.itemName}' stacked in {slot.name}. New Qty: {slot.myItemUI.CurrentQuantity}");
                    return; // 스택 성공
                }
            }
            // === 인벤토리 슬롯 확인 (스택 로직) ===
            foreach (var slot in inventorySlots)
            {
                if (slot.myItemData == item && slot.myItemUI != null && slot.myItemUI.CurrentQuantity < item.maxStackSize)
                {
                    slot.myItemUI.CurrentQuantity++; // 수량 증가
                    Debug.Log($"SUCCESS: '{item.itemName}' stacked in inventory slot {slot.name}. New Qty: {slot.myItemUI.CurrentQuantity}");
                    return; // 아이템 추가 완료
                }
            }
        }

        // 스택할 수 없거나, 스택 가능한 아이템이지만 모든 기존 슬롯이 꽉 찼을 경우
        // 비어있는 새 슬롯에 아이템을 생성
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].myItemUI == null) // 비어있는 슬롯을 찾음
            {
                var newItemUI = Instantiate(itemPrefab, inventorySlots[i].transform);
                newItemUI.Initialize(item, inventorySlots[i]); // Initialize 호출
                Debug.Log($"새 슬롯에 '{item.itemName}' 추가.");
                return; // 아이템 추가 완료
            }
        }

        Debug.LogWarning($"인벤토리가 가득 찼습니다. '{item.itemName}'을(를) 추가할 수 없습니다.");


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
            //드롭할 아이템의 수량
            int quantityToDrop = itemToDropUI.CurrentQuantity;
            Item droppedItemData = itemToDropUI.myItem;

            //아이템이 떨어질 위치를 계산 (플레이어 전방)
            Transform playerTransform = SamplePlayerManager.Instance.Player.transform; // PlayerController의 transform
            Vector3 playerForward = playerTransform.forward;
            Vector3 dropPosition = playerTransform.position + playerTransform.forward * _dropDistance;
            dropPosition.y += 0.5f;

            RaycastHit hit;
            if (Physics.Raycast(dropPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, _groundLayer))
            {
                dropPosition.y = hit.point.y + 0.1f;
            }

            // 수량만큼 월드 아이템 개별 생성
            for (int i = 0; i < quantityToDrop; i++)
            {
                // 아이템이 떨어질 위치를 조금씩 다르게 하여 겹치지 않게 함
                Vector3 scatteredPosition = dropPosition;
                // 랜덤한 수평 방향으로 살짝 퍼지게 함
                scatteredPosition.x += UnityEngine.Random.Range(-0.5f, 0.5f);
                scatteredPosition.z += UnityEngine.Random.Range(-0.5f, 0.5f);
                scatteredPosition.y += UnityEngine.Random.Range(0f, 0.2f); // 높이도 살짝 다르게

                GameObject worldItemGO = Instantiate(itemWorldPrefab, scatteredPosition, Quaternion.identity);
                WorldItem worldItemScript = worldItemGO.GetComponent<WorldItem>();

                if (worldItemScript != null)
                {
                    // WorldItem의 Initialize 메서드를 호출하여 아이템 데이터만 전달 (수량은 1개로 가정)
                    worldItemScript.Initialize(droppedItemData);

                    // Rigidbody가 있다면 힘을 가해서 좀 더 자연스럽게 퍼지게 할 수 있음
                    Rigidbody rb = worldItemGO.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
                        rb.AddForce(randomDirection * _scatterForce, ForceMode.Impulse);
                    }
                }
                else
                {
                    Debug.LogError($"드롭된 월드 프리팹 '{itemWorldPrefab.name}'에 WorldItem 스크립트가 없습니다!");
                }
            }
            // 인벤토리 슬롯에서 아이템을 제거하고 UI 인스턴스 파괴
            if (itemToDropUI.activeSlot != null)
            {
                itemToDropUI.activeSlot.ClearSlot(); // 해당 슬롯을 비움 (데이터 및 UI 참조 제거)
                Debug.Log($"아이템 '{itemToDropUI.myItem.itemName}' {quantityToDrop}개 모두 슬롯에서 버려졌습니다.");
            }
            else
            {
                Destroy(itemToDropUI.gameObject);
            }

            CarriedItem = null;
        }

        //인벤토리 슬롯에서 아이템을 제거하고 UI 인스턴스 파괴
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

        // 핫바 슬롯에서 제거
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (hotbarSlots[i].myItemData == itemToRemove)
            {
                // 스택 가능한 아이템이고, 남은 수량이 제거할 수량보다 많으면 수량만 감소
                if (itemToRemove.isStackable && hotbarSlots[i].myItemUI.CurrentQuantity > amount - removedCount)
                {
                    hotbarSlots[i].myItemUI.CurrentQuantity -= (amount - removedCount);
                    removedCount = amount; // 모두 제거된 것으로 간주
                    break;
                }
                else // 스택 불가능하거나, 남은 수량이 제거할 수량 이하이면 슬롯 비움
                {
                    int currentStack = hotbarSlots[i].myItemUI.CurrentQuantity;
                    hotbarSlots[i].ClearSlot();
                    removedCount += currentStack;
                    if (removedCount >= amount) break;
                }
            }
        }

        // 인벤토리 슬롯에서 제거 (핫바에서 전부 제거되지 않은 경우)
        if (removedCount < amount)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].myItemData == itemToRemove)
                {
                    // 스택 가능한 아이템이고, 남은 수량이 제거할 수량보다 많으면 수량만 감소
                    if (itemToRemove.isStackable && inventorySlots[i].myItemUI.CurrentQuantity > amount - removedCount)
                    {
                        inventorySlots[i].myItemUI.CurrentQuantity -= (amount - removedCount);
                        removedCount = amount;
                        break;
                    }
                    else // 스택 불가능하거나, 남은 수량이 제거할 수량 이하이면 슬롯 비움
                    {
                        int currentStack = inventorySlots[i].myItemUI.CurrentQuantity;
                        inventorySlots[i].ClearSlot();
                        removedCount += currentStack;
                        if (removedCount >= amount) break;
                    }
                }
            }
        }

        Debug.Log($"{itemToRemove.name} {removedCount}개를 인벤토리에서 제거했습니다.");
        if (removedCount < amount)
        {
            Debug.LogWarning($"요청한 {amount}개 중 {amount - removedCount}개를 제거하지 못했습니다. 아이템 부족.");
        }
    }

    //Item PickRandomItem()
    //{
    //    int random = Random.Range(0, items.Length);
    //    return items[random];
    //}
}
