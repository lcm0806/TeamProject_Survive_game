using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    Image itemIcon;
    public CanvasGroup canvasGroup { get; private set; }

    public Item myItem { get; set; }
    public InventorySlot activeSlot { get; set; }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemIcon = GetComponent<Image>();

        if (canvasGroup == null) Debug.LogWarning("InventoryItem: CanvasGroup이 없습니다! " + gameObject.name);
        if (itemIcon == null) Debug.LogWarning("InventoryItem: Image 컴포넌트가 없습니다! " + gameObject.name);
    }

    public void Initialize(Item item, InventorySlot parent)
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        if (itemIcon == null) // <-- 이 부분이 핵심입니다.
        {
            itemIcon = GetComponent<Image>();
        }

        if (itemIcon == null)
        {
            Debug.LogError("InventoryItem: Initialize에서 Image 컴포넌트를 찾을 수 없습니다. 프리팹 설정을 확인하세요.");
            return; // 더 이상 진행하지 않음
        }

        activeSlot = parent;
        activeSlot.myItem = this;
        myItem = item;
        itemIcon.sprite = item.sprite;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1; // 보이게
            canvasGroup.blocksRaycasts = true; // 레이캐스트 허용 (드래그 가능하도록)
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            Inventory.Instance.SetCarriedItem(this);
        }
    }
}
