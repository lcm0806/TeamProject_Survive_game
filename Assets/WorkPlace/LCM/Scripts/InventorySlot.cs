using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public enum SlotTag { None, Head, Chest, Legs, Feet }

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Item myItemData { get; private set; }
    public InventoryItem myItemUI { get; set; }

    public SlotTag myTag;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem droppedItemUI = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (droppedItemUI != null)
        {
            SetItem(droppedItemUI); // 현재 슬롯에 아이템 설정 (자동으로 CarriedItem 해제)
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if(Inventory.CarriedItem == null) return;
            if(myTag != SlotTag.None && Inventory.CarriedItem.myItem.itemTag != myTag) return;
            SetItem(Inventory.CarriedItem);
        }
    }

    public void SetItem(InventoryItem itemUI)
    {
        // 기존에 슬롯에 있던 아이템이 있다면 처리 (드래그 중인 아이템이 아니라, 슬롯 자체의 이전 아이템)
        if (myItemUI != null)
        {
            // TODO: 기존 아이템 UI를 비활성화하거나 파괴하는 로직 추가
            // Destroy(myItemUI.gameObject); // 필요하다면 이전 아이템 UI를 파괴
        }
        if (itemUI.activeSlot != null)
        {
            itemUI.activeSlot.myItemData = null; // 이전 슬롯의 데이터 비움
            itemUI.activeSlot.myItemUI = null; // 이전 슬롯의 UI 참조 비움
        }

        Inventory.CarriedItem = null;

        myItemData = itemUI.myItem;

        myItemUI = itemUI;
        myItemUI.activeSlot = this; // 아이템 UI가 현재 슬롯을 참조하도록 설정

        myItemUI.transform.SetParent(transform);
        myItemUI.transform.localPosition = Vector3.zero; // 위치 초기화
        myItemUI.canvasGroup.blocksRaycasts = true;


    }

    public void ClearSlot()
    {
        if (myItemUI != null)
        {
            Destroy(myItemUI.gameObject); // UI 인스턴스를 파괴
            myItemUI = null;
        }
        myItemData = null; // 데이터도 비움
        // UI를 시각적으로 초기화하는 로직 (예: 이미지 숨기기)
    }
}
