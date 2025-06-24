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

    void Start()
    {
    }

    void Update()
    {

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
