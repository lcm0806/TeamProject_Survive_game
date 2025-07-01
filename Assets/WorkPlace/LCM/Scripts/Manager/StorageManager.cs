using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public GameObject inventorySlotPrefab; // 인벤토리 슬롯 프리팹
    public Transform contentParent;         // Scroll Rect의 Content 오브젝트

    public int numberOfSlotsToCreate = 50; // 생성할 슬롯의 개수 (예시)

    // Start is called before the first frame update
    void Start()
    {
        GenerateInventorySlots(numberOfSlotsToCreate);
    }

    public void GenerateInventorySlots(int count)
    {
        // 기존 슬롯 제거 (옵션)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 슬롯 생성
        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, contentParent);
            // 여기에 필요하다면 슬롯에 데이터(예: 아이템 정보)를 설정하는 로직 추가
            // 예: slot.GetComponent<InventorySlotUI>().SetItem(inventoryItems[i]);
        }

        // ContentSizeFitter가 즉시 업데이트되지 않을 수 있으므로 RebuildLayoutGroup 호출
        // LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
    }
}
