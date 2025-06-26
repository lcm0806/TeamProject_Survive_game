using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [Tooltip("이 3D 오브젝트가 나타내는 Item ScriptableObject 데이터")]
    public Item itemData;

    [Header("Interaction Settings")]
    [Tooltip("플레이어가 상호작용 가능한 범위")]
    public float interactionRange = 0.5f;

    public void Initialize(Item item)
    {
        itemData = item;
        // 필요하다면 아이템 종류에 따라 시각적 요소 (SpriteRenderer 등)를 설정할 수 있습니다.
        // 예: GetComponent<SpriteRenderer>().sprite = item.WorldSprite;
    }

    public void Interact()
    {
        if(itemData == null)
        {
            Debug.Log("아이템 데이터가 할당되지 않았습니다.");
        }

        Debug.Log("아이템과 상호작용 했습니다. 인벤토리에 추가 하겠습니다");

        Inventory.Instance.SpawnInventoryItem(this.itemData);

        Destroy(gameObject);
    }
}
