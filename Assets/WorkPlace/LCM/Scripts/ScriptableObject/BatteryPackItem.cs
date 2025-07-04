using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Consumable/Battery Pack")]
public class BatteryPackItem : ConsumableItem
{
    [Header("Battery Pack Specifics")]
    [Tooltip("이 아이템 사용 시 회복될 전기량입니다.")]
    public double ElectricRestoreAmount;

    public override void Use(GameObject user)
    {
        base.Use(user); // 디버깅용

        Debug.Log($"{itemName}을(를) 사용하여 전기를 {ElectricRestoreAmount}만큼 회복했습니다.");

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ElecticGauge.Value += ElectricRestoreAmount;
        }
        else
        {
            Debug.LogWarning("PlayerManager.Instance를 찾을 수 없습니다. 전기 회복 실패.");
        }
    }
}
