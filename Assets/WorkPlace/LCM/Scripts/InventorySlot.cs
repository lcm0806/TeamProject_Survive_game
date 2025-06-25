using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SlotTag { None, Head, Chest, Legs, Feet }

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public InventoryItem myItem { get; set; }

    public SlotTag myTag;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if(Inventory.CarriedItem == null) return;
            if(myTag != SlotTag.None && Inventory.CarriedItem.myItem.itemTag != myTag) return;
            SetItem(Inventory.CarriedItem);
        }
    }

    public void SetItem(InventoryItem item)
    {
        Inventory.CarriedItem = null;

        // Reset old slot
        item.activeSlot.myItem = null;

        // Set current slot
        myItem = item;
        myItem.activeSlot = this;
        myItem.transform.SetParent(transform);
        myItem.canvasGroup.blocksRaycasts = true;

        if(myTag != SlotTag.None)
        { Inventory.Instance.EquipEquipment(myTag, myItem); }
    }
}
