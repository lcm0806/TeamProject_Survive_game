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


    //플레이어 감지
    //private GameObject player; // 플레이어 오브젝트에 대한 참조 , 거리측정시 사용
    //private bool playerInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        //플레이어 감지
        //player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //플레이어 감지
        //if (player != null)
        //{
        //    float distance = Vector3.Distance(transform.position, player.transform.position);
        //    bool wasPlayerInRange = playerInRange;
        //    playerInRange = distance <= interactionRange;

        //    if (playerInRange && !wasPlayerInRange)
        //    {
        //        Debug.Log($"플레이어가 아이템 근처에 있습니다");
        //    }

        //    else if (!playerInRange && wasPlayerInRange)
        //    {
        //        Debug.Log($"아이템의 범위에서 벗어났습니다.");
        //    }
        //}
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
