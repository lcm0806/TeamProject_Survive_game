using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//모든 게임 이벤트들의 데이터


[CreateAssetMenu(fileName = "GameEventData", menuName = "Event System/Game Event")]
public class GameEventData : ScriptableObject
{
    


    public static GameEventData Instance { get; private set; }

    public int id;
    public string title;
    [TextArea] public string description;


    [Header("이벤트 효과(밤마다 발동되는 불이익)")]
    public double minusCurDurability;
    public int curOxygen;
    public int curEnergy;
    public int BatteryEfficiency; 
    public int OxygenEfficiency;

    [Header("종료 조건(필요 아이템 등)")]
    public string requiredItem; //요구 아이템이 두가지 이상인데..어떻게하지
    public int requiredAmount; 

    [Header("첫날 + 매일 기본적으로 재생성되는 이벤트")]
    public bool reactiveAfterEnd;

    [Header("미완료상태로 날짜전환시 출력 대사")]
    [TextArea] public string dialogue;
}
