using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//전체 이벤트 시스템을 관리, 이벤트 데이터 로드, 이벤트 활성화, 진행 상태 업데이트, 이벤트완료 등
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private Dictionary<int, GameEventData> eventDict = new();
    private int _curGameDay;
    private GameEventData _curEventData;

    public List<GameEventData> CurEvents = new(); //10001등 오늘 랜덤발생한 id의 이벤트
    public List<Button> CurEventButtons = new();
    public List<Button> FinishedButtons = new();
    [SerializeField] private Transform eventContents;
    [SerializeField] private Button eventButtonPrefab;
    [SerializeField] private EventUI eventUI;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCurrentDay();
        LoadAllEvents();
    }

    private void Start()
    {
        EventStart();
    }

    private void InitializeCurrentDay()
    {
        if (StatusSystem.Instance != null)
        {
            _curGameDay = StatusSystem.Instance.GetCurrentDay();
        }
        else
        {
            Debug.LogWarning("StatusSystem.Instance is not available yet. Using default value for current day.");
            _curGameDay = 1;
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

    public void EventStart() // 일자가 시작될때 호출.
    {
        ButtonClear();
        GenerateDailyEvent();
        GenerateRandomEvent();
        FinishedButtonInstantiate();
    }

    public void ButtonClear()
    {
        for (int i = 0; i < eventContents.childCount; i++)
        {
            Destroy(eventContents.GetChild(i));
        }

    }

    public void FinishedButtonInstantiate()
    {
        foreach (Button bt in FinishedButtons)
        {
            Instantiate(bt).gameObject.transform.SetParent(eventContents);
            bt.interactable = false;
        }

    }
    public void GenerateDailyEvent() //아침시점에 호출되어야 함
    {
        //중복발생불가 로직이 필요한가?
        CurEvents.Add(eventDict[10001]);
        CurEvents[CurEvents.Count - 1].GenerateRandomDuraValue();
        CurEvents[CurEvents.Count - 1].isComplete = false;
        EventButtonCreate(CurEvents.Count - 1);
        Debug.Log("내구도 수리 이벤트 발생(매일)");
    }

    public void EventButtonCreate(int eventIndex)
    {
        CurEventButtons.Add(Instantiate(eventButtonPrefab));
        CurEventButtons[eventIndex].transform.SetParent(eventContents);
        CurEventButtons[eventIndex].onClick
            .AddListener(() => eventUI.SetEventListTitleText(CurEvents[eventIndex], eventIndex));
    }

    public void GenerateRandomEvent() // ******아침시점에 호출되어야함 씬변경관련시스템살펴보기, 1~3일차는 호출1번, 4~6일차에는 호출2번, 7일차~호출2번
    {
        int eventID = 0;
        if (3 >= StatusSystem.Instance.GetCurrentDay())
        {
            eventID = Random.Range(10002, 10005); //1~3일차에 id 10002~10004 중 1개
            Debug.Log("1~3일차 이벤트 발생");
            CurEvents.Add(eventDict[eventID]);
            CurEvents[CurEvents.Count - 1].isComplete = false;

            EventButtonCreate(CurEvents.Count - 1);
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            if (6 >= StatusSystem.Instance.GetCurrentDay())
            {
                eventID = Random.Range(10002, 10007); //4~6일차에 id10002~10006 중 2개
                Debug.Log("4~6일차 이벤트 발생");
            }
            else
            {
                eventID = Random.Range(10005, 10010); //7일차~ id10005~10009 중 2개
                Debug.Log("7일차~ 이벤트 발생");
            }

            CurEvents.Add(eventDict[eventID]);
            CurEvents[CurEvents.Count - 1].isComplete = false;

            EventButtonCreate(CurEvents.Count - 1);
        }
    }

    // 이벤트가 완료가능인지 판별
    public bool DetermineEventComplete(GameEventData data) //*버튼에 호출 달아줘야함
    {
        if (data.requiredItemA != null && data.requiredItemB == null)
            return data.requiredAmountA <= Storage.Instance.GetItemCount(data.requiredItemA);

        if (data.requiredItemA != null && data.requiredItemB != null)
            return data.requiredAmountB <= Storage.Instance.GetItemCount(data.requiredItemB);

        return false;
    }

    public void EventEffect(GameEventData data) //******날짜가 넘어갈때 적용되어야함 이함수가 어디선가 호출되어야 함 => 날짜넘어가는시스템쪽 확인해서 집어넣기
    {
        if (data.isComplete)
        {
            StatusSystem.Instance.SetPlusDurability(data.PlusDurability);
            StatusSystem.Instance.SetPlusOxygenGainMultiplier(data.PlusOxygenEfficiency);
            StatusSystem.Instance.SetPlusEnergyGainMultiplier(data.PlusEnergyEfficiency);
        }
        else
        {
            StatusSystem.Instance.SetMinusDurability(data.RandomMinusDuraValue);
            StatusSystem.Instance.SetMinusDurability(data.MinusDurability);
            StatusSystem.Instance.SetMinusEnergy(data.MinusEnergy);
            StatusSystem.Instance.SetMinusOxygen(data.MinusOxygen);
            StatusSystem.Instance.SetMinusOxygenGainMultiplier(data.MinusOxygenEfficiency);
            StatusSystem.Instance.SetMinusEnergyGainMultiplier(data.MinusEnergyEfficiency);
        }

    }

    public void EventClear(GameEventData data, int eventIndex)
    {
        Storage.Instance.RemoveItem(data.requiredItemA, data.requiredAmountA);
        if (data.requiredItemB != null)
            Storage.Instance.RemoveItem(data.requiredItemB, data.requiredAmountB);
        CurEventButtons[eventIndex].interactable = false;
        FinishedButtons.Add(CurEventButtons[eventIndex]);
        CurEvents.RemoveAt(eventIndex);
        CurEventButtons.RemoveAt(eventIndex);
    }
}