using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//모든 퀘스트 데이터를 저장

public enum TriggerPhase {Start, Early, Mid}
[CreateAssetMenu(fileName = "GameEventData", menuName = "Event System/Game Event")]
public class GameEventData :ScriptableObject
{
    public int id;
    public string title;
    [TextArea] public string description;
    public TriggerPhase triggerphase;

    [Header("이벤트 효과(밤마다 발동되는 불이익")]
    [TextArea] public string effectDescription;

    [Header("종료 조건(필요 아이템 등")]
    public string requiredItem;
    public int requiredAmount;

    [Header("종료 후 처리(보상 또는 회복")]
    [TextArea] public string endEffectDescription;
    public bool reactiveAfterEnd;

    [Header("날짜 전환 시 대사")]
    [TextArea] public string dialogue;
}
