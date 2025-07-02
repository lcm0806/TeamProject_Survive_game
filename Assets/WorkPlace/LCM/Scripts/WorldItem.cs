using System.Collections;
using System.Collections.Generic;
using Test;
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
        
        if (itemData == null)
        {
            Debug.Log("아이템 데이터가 할당되지 않았습니다.");
        }
        Debug.Log($"[WorldItem] Interact() 호출됨. 아이템: {itemData?.itemName ?? "Unknown"}");
        Debug.Log("아이템과 상호작용 했습니다. 인벤토리에 추가 하겠습니다");

        Inventory.Instance.SpawnInventoryItem(this.itemData);

        PlayerManager.Instance.SelectItem = this.itemData;

        ToolItem toolItem = this.itemData as ToolItem;

        if (toolItem != null)
        {
            // GameObject toolObject = Instantiate(toolItem.toolPrefab);
            // toolItem.toolObject = toolObject;
            // toolItem.toolAction = toolObject.GetComponent<TestToolAction>();
            // toolObject.SetActive(false);
            // 혹은 해당 핫바를 선택할때 마다 생성 및 파괴
            toolItem.toolAction = toolItem.HandleItem.GetComponent<ToolAction>();
        }
        Debug.Log($"[WorldItem] Destroy(gameObject) 호출 직전. 파괴될 오브젝트: {gameObject.name}");
        Destroy(gameObject);
        Debug.Log($"[WorldItem] Destroy(gameObject) 호출 완료.");
    }
}
