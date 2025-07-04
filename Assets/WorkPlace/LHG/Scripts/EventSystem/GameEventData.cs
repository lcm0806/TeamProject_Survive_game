using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//모든 게임 이벤트들의 데이터


[CreateAssetMenu(fileName = "GameEventData", menuName = "Event System/Game Event")]
public class GameEventData : ScriptableObject
{
    public int id;
    public string title;
    [TextArea] public string description;
    public string eventEffectDesc; //이벤트의 (부정적)효과
    public string eventRquirementDesc; //이벤트의 종료조건(필요물건)
    private int minDura = 10;
    private int maxDura = 20;


    public void GenerateRandomDuraValue()
    {
        RandomMinusDuraValue = Random.Range(minDura, maxDura + 1);
    }

    public bool isComplete;

    [Header("이벤트 효과(밤마다 발동되는 불이익)")]
    public double RandomMinusDuraValue;
    public double PlusDurability;
    public double MinusDurability;
    public double MinusOxygen;
    public double MinusEnergy;
    public float PlusEnergyEfficiency;
    public float MinusEnergyEfficiency;
    public float PlusOxygenEfficiency;
    public float MinusOxygenEfficiency;

    [Header("종료 조건(필요 아이템 등)")]
    public Item requiredItemA;
    public int requiredAmountA;
    public Item requiredItemB;
    public int requiredAmountB;

    [Header("첫날 + 매일 기본적으로 재생성되는 이벤트")]
    public bool reactiveAfterEnd;

    [Header("미완료상태로 날짜전환시 출력 대사")]
    [TextArea] public string dialogue;

    
    public enum EventState
    {
        //상태 변수명 나중에 다시 통일
        CanNotComplete,
        CanComplete,
        AlreadyComplted
    }

    public enum EventIsActivated //버튼? 이벤트?
    {
        NotExist,
        Activated,
        Disabled
    }

    public EventIsActivated isActivated;//이벤트활성화여부?
}