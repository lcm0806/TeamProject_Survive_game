using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Consumable/Air Tank")]
public class AirTankItem : ConsumableItem
{
    [Header("Air Tank Specifics")]
    [Tooltip("이 아이템 사용 시 회복될 산소량입니다.")]
    public float OxygenRestoreAmount;

    public override void Use(GameObject user)
    {
        base.Use(user); //디버깅 용

        Debug.Log($"{OxygenRestoreAmount}산소를 회복했습니다.");
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.AirGauge.Value += OxygenRestoreAmount;
        }
        else
        {
            Debug.LogWarning("PlayerManager.Instance를 찾을 수 없습니다. 산소 회복 실패.");
        }
    }
}
