using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInventoryAdder : MonoBehaviour
{
    [SerializeField] private List<Item> debugItems; // 인스펙터에서 넣고 싶은 Item ScriptableObject들을 할당
    [SerializeField] private int debugQuantity = 10; // 한번에 추가할 기본 수량

    // 이 버튼을 UI에 연결하거나 에디터에서 직접 호출
    public void AddDebugItemsToStorage()
    {
        if (Storage.Instance == null)
        {
            Debug.LogError("Storage.Instance가 없습니다!");
            return;
        }

        foreach (Item item in debugItems)
        {
            if (item != null)
            {
                Storage.Instance.AddItemToStorage(item, debugQuantity);
                Debug.Log($"{item.name} {debugQuantity}개를 창고에 추가했습니다.");
            }
        }
    }

    // 특정 아이템을 추가하는 메서드 (선택 사항)
    public void AddSpecificItemToStorage(Item item, int quantity)
    {
        if (Storage.Instance == null)
        {
            Debug.LogError("Storage.Instance가 없습니다!");
            return;
        }
        if (item != null)
        {
            Storage.Instance.AddItemToStorage(item, quantity);
            Debug.Log($"{item.name} {quantity}개를 창고에 추가했습니다.");
        }
    }
}
