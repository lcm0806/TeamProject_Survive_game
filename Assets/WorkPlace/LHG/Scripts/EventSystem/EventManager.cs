using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//전체 이벤트 시스템을 관리, 이벤트 데이터 로드, 이벤트 활성화, 진행 상태 업데이트, 이벤트완료 등
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private Dictionary<int, GameEventData> eventDict = new();
    public List<GameEventData> allEvents = new();
    public List<GameEventData> onWorkingEvents = new();


    private int _curGameDay = StatusSystem.Instance.GetCurrentDay();

    //인벤토리 및 창고 참조

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllEvents();
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
                allEvents.Add(e);
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

    //현재 발동된 이벤트들의 리스트 있어야 할듯 ? onWorkingEvents
    



    // 이벤트가 발동 또는 생성되는 로직
    public void TriggerEvent()
    {
        // 매일 아침 현재 발동중인 이벤트를 전체를 살펴보고
        // 오늘이 며칠째인지 판별하고
        // 일자에 맞는 이벤트id범위의 SO에서 랜덤하게 발동

        // 단, 중복되는 id는 발동시키지 않음

        // 단, 게임 시작부터 매일 재생성되는 이벤트 id 10001
        if (GameEventData.Instance.reactiveAfterEnd == true)
        {

        }

        //1~3일차에 id 10002~10004 중 1개

        //4~6일차에 id10002~10006 중 2개

        //7일차~ id10005~10009 중 2개

    }




    // 이벤트가 완료가능인지 판별
    public void DetermineEventComplete()
    {

        // if required item count < 인벤토리의 cur_item_count이면 item count --해주고
        // 해당 id의 이벤트 자체를 제거

    }


    // 












}
