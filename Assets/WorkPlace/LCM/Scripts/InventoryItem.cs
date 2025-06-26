using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Image itemIcon;
    public CanvasGroup canvasGroup { get; private set; }

    public Item myItem { get; set; }
    public InventorySlot activeSlot { get; set; }

    [SerializeField] private TextMeshProUGUI quantityText;
    private int _currentQuantity;
    public int CurrentQuantity
    {
        get { return _currentQuantity; }
        set
        {
            _currentQuantity = value;
            if (quantityText != null)
            {
                // 수량이 1개 이하면 숨기고, 아니면 표시
                quantityText.gameObject.SetActive(_currentQuantity > 1);
                quantityText.text = _currentQuantity.ToString();
            }
        }
    }
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        itemIcon = GetComponent<Image>();
        // quantityText = GetComponentInChildren<TMP_Text>();

        if (canvasGroup == null) Debug.LogWarning("InventoryItem: CanvasGroup이 없습니다! " + gameObject.name);
        if (itemIcon == null) Debug.LogWarning("InventoryItem: Image 컴포넌트가 없습니다! " + gameObject.name);
    }

    public void Initialize(Item item, InventorySlot parent)
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        if (itemIcon == null) 
        {
            itemIcon = GetComponent<Image>();
        }

        if (itemIcon == null)
        {
            Debug.LogError("InventoryItem: Initialize에서 Image 컴포넌트를 찾을 수 없습니다. 프리팹 설정을 확인하세요.");
            return; // 더 이상 진행하지 않음
        }

        activeSlot = parent;
        activeSlot.myItemUI = this;
        activeSlot.myItemData = item;
        myItem = item;
        itemIcon.sprite = item.icon;
        CurrentQuantity = 1;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1; // 보이게
            canvasGroup.blocksRaycasts = true; // 레이캐스트 허용 (드래그 가능하도록)
        }

        transform.SetParent(parent.transform); // 초기 부모 설정
        transform.localPosition = Vector3.zero; //위치 초기화

    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if(eventData.button == PointerEventData.InputButton.Left)
    //    {
    //        Inventory.Instance.SetCarriedItem(this);
    //    }
    //}

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myItem != null)
        {
            // SampleUIManager (또는 Inventory)의 SetItemDescription 메서드를 호출하여 설명을 표시합니다.
            SampleUIManager.Instance.SetItemDescription(myItem.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 아이템 위에서 벗어나면 설명을 지웁니다.
        SampleUIManager.Instance.SetItemDescription("");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup == null) return;

        // 마우스로 들고 다니는 아이템으로 설정
        Inventory.Instance.SetCarriedItem(this); 

        // 드래그 중에는 이 아이템 UI가 다른 UI 요소들의 레이캐스트를 막지 않도록 합니다.
        canvasGroup.blocksRaycasts = false;
        // 드래그 중에는 잠시 부모를 Canvas의 최상위 (DraggablesTransform)로 변경하여 UI 계층에 따라 가려지지 않도록 합니다.
        transform.SetParent(Inventory.Instance.draggablesTransform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 원래 슬롯으로 돌아가도록 처리합니다.
        if (Inventory.CarriedItem != null)
        {
            // InventorySlot의 SetItem 메서드를 다시 호출하여 원래 슬롯으로 돌려놓습니다.
            activeSlot.SetItem(Inventory.CarriedItem);
        }
    }
}
