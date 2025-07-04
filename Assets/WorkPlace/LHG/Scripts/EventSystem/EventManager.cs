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

    public List<GameEventData> CurEvents = new(); 
    public List<Button> CurEventButtons = new(); 
    public List<Button> FinishedButtons = new(); 
    [SerializeField] private Transform eventContents;
    [SerializeField] private Button eventButtonPrefab;
    [SerializeField] private EventUI eventUI;


    //참조연결 지붕조각 연결하기 **테스트용코드 반드시 삭제**
    [SerializeField] Item testItem;

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

    private void Update()
    {
        //**테스트용 코드 반드시 삭제**
        if (Input.GetKeyDown(KeyCode.H))
        {

            Storage.Instance.AddItemToStorage(testItem, 1);
            Debug.Log($"{testItem.name}창고에 추가됨");
        }
        //**테스트용 코드 반드시 삭제**
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
        GenerateDailyEvent();
        GenerateRandomEvent();
        DettachButton();
        AttachButton(CurEventButtons);
        AttachButton(FinishedButtons);
    }

    public void DettachButton()
    {
        for (int i = 0; i < eventContents.childCount; i++)
        {
            eventContents.DetachChildren();
        }
    }
    public void AttachButton(List<Button> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            list[i].transform.SetParent(eventContents);
            int temp = i; //??
            if(list == CurEventButtons)
            {
                list[i].onClick.RemoveAllListeners();
                list[i].onClick.AddListener(() => eventUI.SetEventListTitleText(CurEvents[temp], temp)); //0704 
                
            }

        }
    }
    

    public void GenerateDailyEvent() 
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
        eventUI.SetEventSubUIBtnTitle(CurEventButtons[eventIndex].gameObject, eventIndex); //추가한부분**서브타이틀용**
    }

    public void GenerateRandomEvent() 
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
    public bool DetermineEventComplete(GameEventData data) 
    {
        if (data.requiredItemA != null && data.requiredItemB == null)
            return data.requiredAmountA <= Storage.Instance.GetItemCount(data.requiredItemA);

        if (data.requiredItemA != null && data.requiredItemB != null)
            return data.requiredAmountB <= Storage.Instance.GetItemCount(data.requiredItemB);

        return false;
    }

    public void EventEffect(GameEventData data) 
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

        DettachButton();
        AttachButton(CurEventButtons);
        AttachButton(FinishedButtons);

        //Storage.Instance.RemoveItem(data.requiredItemA, data.requiredAmountA);
        //if (data.requiredItemB != null)
        //    Storage.Instance.RemoveItem(data.requiredItemB, data.requiredAmountB);
        //CurEventButtons[eventIndex].interactable = false;
        //FinishedButtons.Add(CurEventButtons[eventIndex]);
        //CurEvents.RemoveAt(eventIndex);

        //CurEventButtons[eventIndex].transform.SetParent(eventContents); //추가

        //CurEventButtons.RemoveAt(eventIndex);




        //ButtonClear(); //0704 추가
        //EventButtonInstantiate();//0704 추가
        //FinishedButtonInstantiate();//0704 추가
    }
}
