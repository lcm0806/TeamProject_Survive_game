using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //전체 이벤트 시스템을 관리, 이벤트 데이터 로드, 이벤트 활성화, 진행 상태 업데이트, 이벤트완료 등

    // 이벤트 데이터 목록을 가져오고
    public static EventManager Instance { get; private set; }
    private Dictionary<int, GameEventData> eventDict = new();
    public List<GameEventData> allEvents = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllEvents();
    }

    private void LoadAllEvents()
    {
        var events = Resources.LoadAll<GameEventData>("Events");
        foreach(var e in events)
        {
            if(!eventDict.ContainsKey(e.id))
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


    public List<GameEventData> GetEventsByPhase(TriggerPhase phase)
    {
        return allEvents.FindAll(e => e.triggerphase == phase);
    }

    private void TriggerRandomEvent(int gameDay)
    {
        TriggerPhase phase = GetPhaseFromDay(gameDay);
        var candidates = EventManager.Instance.GetEventsByPhase(phase);

        if (candidates.Count == 0) return;

        int index = Random.Range(0, candidates.Count);
        GameEventData todayEvent = candidates[index];

        Debug.Log($"오늘의 이벤트 :{todayEvent.title} - {todayEvent.description}");
    }

    TriggerPhase GetPhaseFromDay(int day)
    {
        if (day == 1) return TriggerPhase.Start;
        if (day <= 3) return TriggerPhase.Early;
        return TriggerPhase.Mid;
    }

    //인벤토리 및 창고 참조


    // 이벤트 발생



    // 이벤트 진행상태판별

    // 이벤트 완료처리


}
