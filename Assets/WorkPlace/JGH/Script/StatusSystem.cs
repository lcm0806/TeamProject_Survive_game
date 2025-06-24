using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    
    // static = 게임 어디서든 접근 가능한 변수
    private static StatusSystem _instance;
    
    // 다른 스크립트에서 GameManager.Instance 로 접근할 수 있게 해주는 속성
    public static StatusSystem Instance
    {
        get
        {
            // 만약 instance가 없다면 찾아보고, 그래도 없으면 새로 만듭니다
            if (_instance == null)
            {
                _instance = FindObjectOfType<StatusSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StatusSystem");
                    _instance = go.AddComponent<StatusSystem>();
                }
            }
            return _instance;
        }
    }
    
    public int CurrentDay = 1;              // 현재 날짜 (1일부터 시작)
    public float OxygenRemaining = 100f;    // 남은 산소량 (100에서 시작)
    public bool HasExploredToday = false;   // 오늘 탐색했는지 여부
    public List<string> Inventory = new List<string>();  // 수집한 아이템 목록
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
