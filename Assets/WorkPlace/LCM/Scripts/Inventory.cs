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
    [SerializeField] InventorySlot[] equipmentSlots;

    [SerializeField] Transform draggablesTransform;
    [SerializeField] InventoryItem itemPrefab;

    [Header("Item List")]
    [SerializeField] Item[] items;

    //[Header("Debug")]
    //[SerializeField] Button giveItemBtn;

    // --- 새로 추가된 UI 관련 필드 ---
    [Header("UI Management")]
    [SerializeField] private GameObject _inventoryUIRootPanel; // 인벤토리 UI의 최상위 GameObject (Panel 등)


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
    }

    void Update()
    {
        if(CarriedItem == null) return;

        CarriedItem.transform.position = Input.mousePosition;
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

        if(item.activeSlot.myTag != SlotTag.None)
        { EquipEquipment(item.activeSlot.myTag, null); }

        CarriedItem = item;
        CarriedItem.canvasGroup.blocksRaycasts = false;
        item.transform.SetParent(draggablesTransform);
    }

    public void EquipEquipment(SlotTag tag, InventoryItem item = null)
    {
        switch (tag)
        {
            case SlotTag.Head:
                if(item == null)
                {
                    // Destroy item.equipmentPrefab on the Player Object;
                    Debug.Log("Unequipped helmet on " + tag);
                }
                else
                {
                    // Instantiate item.equipmentPrefab on the Player Object;
                    Debug.Log("Equipped " + item.myItem.name + " on " + tag);
                }
                break;
            case SlotTag.Chest:
                break;
            case SlotTag.Legs:
                break;
            case SlotTag.Feet:
                break;
        }
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
            if (inventorySlots[i].myItem == null)
            {
                Instantiate(itemPrefab, inventorySlots[i].transform).Initialize(_item, inventorySlots[i]);
                break;
            }
        }


    }

        //Item PickRandomItem()
        //{
        //    int random = Random.Range(0, items.Length);
        //    return items[random];
        //}
}
