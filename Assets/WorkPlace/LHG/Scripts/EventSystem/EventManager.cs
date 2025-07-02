using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//전체 이벤트 시스템을 관리, 이벤트 데이터 로드, 이벤트 활성화, 진행 상태 업데이트, 이벤트완료 등
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private Dictionary<int, GameEventData> eventDict = new();
    public List<GameEventData> allEvents = new();


    // 250702 빌드시 오류나서 변경
    // private int _curGameDay = StatusSystem.Instance.GetCurrentDay();
    private int _curGameDay;


    //인벤토리 및 창고 참조

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize _curGameDay safely in Awake
        // 250702 빌드시 오류나서 추가
        InitializeCurrentDay();
        LoadAllEvents();
    }
    
    // 250702 빌드시 오류나서 추가
    private void InitializeCurrentDay()
    {
        // Check if StatusSystem.Instance is available
        if (StatusSystem.Instance != null)
        {
            _curGameDay = StatusSystem.Instance.GetCurrentDay();
        }
        else
        {
            Debug.LogWarning("StatusSystem.Instance is not available yet. Using default value for current day.");
            _curGameDay = 1; // or whatever default value makes sense
        }
    }

    // 모든 이벤트 데이터 목록을 가져오고
    private void LoadAllEvents()
    {
        var events = Resources.LoadAll<GameEventData>("Events");
        foreach (var e in events)
        {
            if (!eventDict.ContainsKey(e.id))
            {
                eventDict[e.id] = e;
            }
            else
            {
                Debug.LogWarning($"중복된 이벤트 ID : {e.id}");
            }
        }
        Debug.Log($"이벤트 {eventDict.Count}개 로드 완료");
    }

    public GameEventData GetEventById(int id)
    {
        eventDict.TryGetValue(id, out var result);
        return result;
    }

    //현재 발동된 이벤트들의 목록 있어야 할듯 ? onWorkingEvents => 필요없나봄 


    public void GenerateDailyEvent() //게임시작 + 아침시점에 호출되어야 함
    {
        _curEventData = eventDict[10001];
        Debug.Log("내구도 수리 이벤트 발생(매일)");
    }

    public void GenerateRandomEvent() // ******아침시점에 호출되어야함 씬변경관련시스템살펴보기, 1~3일차는 호출1번, 4~6일차에는 호출2번, 7일차~호출2번
    {
        int eventID = 0;
        if (3 >= StatusSystem.Instance.GetCurrentDay())
        {
            eventID = Random.Range(10002, 10005); //이상미만
            Debug.Log("1~3일차 이벤트 발생");
        }
        else if (6 >= StatusSystem.Instance.GetCurrentDay())
        {
            eventID = Random.Range(10002, 10007);
            Debug.Log("4~6일차 이벤트 발생");
        }
        else
        {
            eventID = Random.Range(10005, 10010);
            Debug.Log("7일차~ 이벤트 발생");
        }

        _curEventData = eventDict[eventID];
        //1~3일차에 id 10002~10004 중 1개

        //4~6일차에 id10002~10006 중 2개

        //7일차~ id10005~10009 중 2개
    }

    // 이벤트가 완료가능인지 판별
    public void DetermineEventComplete() //*버튼에 호출 달아줘야함
    {

        //**아이템보유여부를 파악할수있는 이름이든 id값이든 확인
        //순회로 보유여부 파악 true false 
        // if required item count < 인벤토리의 cur_item_count이면 item count --해주고
        // 해당 id의 이벤트 자체를 제거

    }

    public void EventEffect(GameEventData data) //******날짜가 넘어갈때 적용되어야함 이함수가 어디선가 호출되어야 함 => 날짜넘어가는시스템쪽 확인해서 집어넣기
    {

        if (isEventCompleted() == false)
        {
            //위에서 결정된 게임이벤트  해당을 매개변수로 받아서 적용만
            //내구도 산소 전력을 
            //날짜변경시 종료목표를 달성하지 못하면 적용됨
            StatusSystem.Instance.SetMinusDurability(data.RandomMinusDuraValue);
            StatusSystem.Instance.SetMinusDurability(data.MinusDurability);
            StatusSystem.Instance.SetMinusEnergy(data.MinusEnergy);
            StatusSystem.Instance.SetMinusOxygen(data.MinusOxygen);
            StatusSystem.Instance.SetMinusOxygenGainMultiplier(data.MinusOxygenEfficiency);
            StatusSystem.Instance.SetMinusEnergyGainMultiplier(data.MinusEnergyEfficiency);
        }

        if (isEventCompleted() == true)
        {
            StatusSystem.Instance.SetPlusOxygen(data.PlusDurability);
            StatusSystem.Instance.SetPlusOxygenGainMultiplier(data.PlusOxygenEfficiency);
            StatusSystem.Instance.SetPlusEnergyGainMultiplier(data.PlusEnergyEfficiency);
        }
    }

    public bool isEventCompleted()
    {
        return false;
    }
}
