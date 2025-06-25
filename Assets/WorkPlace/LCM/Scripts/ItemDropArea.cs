using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 게임 오브젝트가 InventoryItem 컴포넌트를 가지고 있는지 확인합니다.
        InventoryItem droppedItemUI = eventData.pointerDrag.GetComponent<InventoryItem>();

        if (droppedItemUI != null)
        {
            if (Inventory.Instance == null) // <- 추가
            {
                Debug.LogError("Error: Inventory.Instance is null when trying to drop item!");
                return; // Inventory 인스턴스가 없으면 더 이상 진행하지 않음
            }
            // Inventory 매니저에게 해당 아이템을 버리라고 요청합니다.
            // 이 메서드는 아래 3단계에서 Inventory 스크립트에 추가할 것입니다.
            Inventory.Instance.DropItemFromSlot(droppedItemUI);
        }
    }
}
