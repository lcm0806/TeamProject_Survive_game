using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("드롭 설정")]
    public GameObject batteryPrefab;
    public GameObject oxygenPrefab;
    public float lootLaunchForce = 5f;

    private enum LootType { Battery, Oxygen }

    private bool hasBeenUsed = false;

    public void Interact()
    {
        if (hasBeenUsed)
        {
            ShowMessage("이미 확인한 보급 상자입니다...");
            Debug.Log("이미 확인한 보급 상자입니다...");
            return;
        }

        hasBeenUsed = true;

        float roll = Random.value; // 0.0 ~ 1.0

        if (roll < 0.3f)
        {
            SpawnLoot(LootType.Battery);
            Debug.Log("건전지");
        }
        else if (roll < 0.6f)
        {
            SpawnLoot(LootType.Oxygen);
            Debug.Log("산소");
        }
        else
        {
            ShowMessage("비어있는 보급 상자입니다...");
            Debug.Log("비어있는 보급 상자입니다...");
        }

    }

    private void SpawnLoot(LootType lootType)
    {
        Vector3 spawnPos = transform.position + transform.up * 1.2f;
        GameObject loot = null;

        switch (lootType)
        {
            case LootType.Battery:
                loot = Instantiate(batteryPrefab, spawnPos, Quaternion.identity);
                break;
            case LootType.Oxygen:
                loot = Instantiate(oxygenPrefab, spawnPos, Quaternion.identity);
                break;
        }

        if (loot != null)
        {
            // 보급상자와 충돌 무시
            Collider myCol = GetComponent<Collider>();
            Collider lootCol = loot.GetComponent<Collider>();
            if (myCol != null && lootCol != null)
            {
                Physics.IgnoreCollision(myCol, lootCol);
            }

            if (loot.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = (Vector3.up + Random.insideUnitSphere * 0.3f).normalized;
                rb.AddForce(dir * lootLaunchForce, ForceMode.Impulse);
            }
        }
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message); // 추후 UI로 교체
        //UIManager.Instance.ShowPopup(message);
    }
}
