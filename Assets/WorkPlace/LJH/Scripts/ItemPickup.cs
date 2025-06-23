using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject pickupUI;  // UI 표시용 (예: "Press E to collect")

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (pickupUI != null)
            pickupUI.SetActive(false);
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance)
        {
            if (pickupUI != null)
                pickupUI.SetActive(true);

            if (Input.GetKeyDown(interactKey))
            {
                Collect();
            }
        }
        else
        {
            if (pickupUI != null)
                pickupUI.SetActive(false);
        }
    }

    void Collect()
    {
        Debug.Log("아이템을 획득했습니다!");

        // 아이템 추가 로직 (예: 인벤토리 추가)

        Destroy(gameObject);  // 파밍포인트 제거
    }
}