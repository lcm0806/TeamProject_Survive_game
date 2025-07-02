using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif

public enum SlotTag { None, Head, Chest, Legs, Feet }

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Item myItemData { get; set; }
    public InventoryItem myItemUI { get; set; }

    public SlotTag myTag;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem droppedItemUI = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (droppedItemUI != null)
        {
            //SetItem(droppedItemUI); // 현재 슬롯에 아이템 설정 (자동으로 CarriedItem 해제)
            Inventory.Instance.HandleItemDropOrClick(this, droppedItemUI);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭 시 아이템 사용 또는 드롭 로직
            if (myItemUI != null && Inventory.CarriedItem == null)
            {
                // 아이템 사용 로직 (예시)
                if (myItemData != null)
                {
                    Debug.Log($"'{myItemData.itemName}'을(를) 사용합니다.");
                    // TODO: 아이템 사용 효과 구현
                    // Inventory.Instance.RemoveItem(myItemData); // 사용 후 아이템 제거 (수량 1 감소)
                }
            }
        }
    }

    public void SetItem(InventoryItem itemUI)
    {
        // 기존에 슬롯에 있던 아이템이 있다면 처리 (드래그 중인 아이템이 아니라, 슬롯 자체의 이전 아이템)
        //if (myItemUI != null)
        //{
        //    // TODO: 기존 아이템 UI를 비활성화하거나 파괴하는 로직 추가
        //    // Destroy(myItemUI.gameObject); // 필요하다면 이전 아이템 UI를 파괴
        //    Destroy(myItemUI.gameObject);
        //}
        //if (itemUI.activeSlot != null)
        //{
        //    itemUI.activeSlot.myItemData = null; // 이전 슬롯의 데이터 비움
        //    itemUI.activeSlot.myItemUI = null; // 이전 슬롯의 UI 참조 비움
        //}

        //Inventory.CarriedItem = null;

        myItemData = itemUI.myItem;
        myItemUI = itemUI;
        myItemUI.activeSlot = this; // 아이템 UI가 현재 슬롯을 참조하도록 설정

        myItemUI.transform.SetParent(transform);
        myItemUI.transform.localPosition = Vector3.zero; // 위치 초기화
        myItemUI.canvasGroup.blocksRaycasts = true;

        // InventoryItem의 현재 수량을 슬롯에 반영 (수량 텍스트 업데이트)
        myItemUI.CurrentQuantity = itemUI.CurrentQuantity; // 이 코드가 InventoryItem의 set property를 호출합니다.


    }

    public void ClearSlot()
    {
        if (myItemUI != null)
        {
            Debug.Log("클리어 슬롯 진행");
            Destroy(myItemUI.gameObject); // UI 인스턴스를 파괴
            myItemUI = null;
        }
        myItemData = null; // 데이터도 비움
        // UI를 시각적으로 초기화하는 로직 (예: 이미지 숨기기)
    }

    public void SetItemData(Item item)
    {
        myItemData = item;
        // myItemUI가 있다면 이 시점에서 UI 업데이트를 트리거할 수도 있습니다.
    }

    public void SetItemQuantity(int quantity)
    {
        if (myItemUI != null)
        {
            myItemUI.CurrentQuantity = quantity;
        }
        // myItemUI가 없다면 오류 처리 또는 데이터만 저장


    }

    public void UpdateSlotUI()
    {
        if (myItemUI != null && myItemData != null)
        {
            // 아이템이 슬롯에 있을 때:
            // InventoryItem의 GameObject를 활성화하여 보이게 합니다.
            if (!myItemUI.gameObject.activeSelf)
            {
                myItemUI.gameObject.SetActive(true);
            }
            // myItemUI.CurrentQuantity = myItemUI.CurrentQuantity; // 이 코드는 불필요
            // myItemUI 내부에 quantityText 업데이트 로직이 있으므로 별도 호출 필요 없음
            // myItemUI 내부에 itemIcon 업데이트 로직이 있으므로 별도 호출 필요 없음
        }
        else
        {
            // 슬롯에 아이템이 없을 때:
            // InventoryItem의 GameObject를 비활성화하여 숨깁니다.
            if (myItemUI != null) // myItemUI가 ClearSlot에 의해 이미 null이 된 경우를 대비
            {
                myItemUI.gameObject.SetActive(false);
            }
            // InventorySlot 자체에 아이콘/수량 텍스트가 있었다면 여기에서 숨기겠지만,
            // 현재 구성상 InventoryItem이 모든 UI를 관리하므로 추가 작업은 필요 없습니다.
        }
    }
}