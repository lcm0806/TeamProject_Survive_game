using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //전체 이벤트 시스템을 관리, 이벤트 데이터 로드, 이벤트 활성화, 진행 상태 업데이트, 이벤트완료 등
    
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

    // 이벤트 데이터 목록을 가져오고
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

    

    public void TriggerEvent()
    {
        if (GameEventData.Instance.reactiveAfterEnd==true)
        {

        }
        //1일차에



        //1~3일차에
        


        //4일차~ 
    }

   

    


    // 이벤트 발생



    // 이벤트 진행상태판별

    // 이벤트 완료처리


}
