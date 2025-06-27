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

        // Inventory에서 CarriedItem 설정
        Inventory.Instance.SetCarriedItem(this);
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
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            bool droppedOnUI = false;
            foreach (var result in results)
            {
                // 드롭 대상이 InventorySlot이거나 InventoryItem(자기 자신 포함)이 아닌 다른 UI 요소일 경우
                // (이 부분은 프로젝트의 UI 구조에 따라 더 정교하게 조정될 수 있습니다.)
                if (result.gameObject.GetComponent<InventorySlot>() != null || result.gameObject.GetComponent<InventoryItem>() != null)
                {
                    droppedOnUI = true;
                    break;
                }
            }
            if (!droppedOnUI) // UI 위에 드롭되지 않았다면 월드에 버림
            {
                Inventory.Instance.DropItemFromSlot(Inventory.CarriedItem);
            }
            else // UI 위에 드롭되었지만 InventorySlot이 아니었거나 드롭 처리되지 않았다면 원래 위치로 되돌림
            {
                // 이전에 들고 있던 아이템의 슬롯으로 다시 되돌림
                if (Inventory.CarriedItem.activeSlot != null)
                {
                    Inventory.CarriedItem.activeSlot.SetItem(Inventory.CarriedItem);
                }
                // 만약 원래 슬롯이 없었다면 (예: 새로 생성된 아이템) 그냥 파괴
                else
                {
                    Destroy(Inventory.CarriedItem.gameObject);
                }
                Inventory.CarriedItem = null; // 들고 있는 아이템 해제
            }
        }
    }
}
