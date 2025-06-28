using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    // 싱글톤 패턴 추가
    private static GameSystem _instance;
    public static GameSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSystem");
                    _instance = go.AddComponent<GameSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private bool isPaused = false;
    private Stack<GameObject> activeMenuStack = new Stack<GameObject>(); // 활성화된 메뉴들을 추적하는 스택
    private Coroutine statusDecreaseCoroutine;

    void Awake()
    {
        // 싱글톤 중복 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 부모가 있다면 루트로 이동
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void Pause()
    {
        Time.timeScale = 0f;  // 게임 시간 정지
        isPaused = true;
        Debug.Log("게임 일시정지");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void Resume()
    {
        Time.timeScale = 1f;  // 게임 시간 재개
        isPaused = false;
        Debug.Log("게임 재개");
    }
    
    /// <summary>
    /// 일시정지 상태 확인
    /// </summary>
    /// <returns></returns>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    private void Start()
    {
        // 코루틴 시작
        if (statusDecreaseCoroutine == null)
        {
            statusDecreaseCoroutine = StartCoroutine(StatusDecreaseCoroutine());
        }
    }

    /// <summary>
    /// 1초에 1씩 스탯 떨어지게
    /// </summary>
    /// <returns></returns>
    private IEnumerator StatusDecreaseCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초마다 실행
       
            // 게임이 일시정지 상태면 감소하지 않음
            if (isPaused) continue;
       
            // 타이틀 씬에서는 감소하지 않음
            string currentScene = SceneSystem.Instance?.GetCurrentSceneName() ?? "";
            if (currentScene == "TitleScene") continue;
       
            // 게임 씬(쉘터, 탐험 등)에서만 스테이터스 감소
            if (currentScene == "ShelterScene" || currentScene == "DevShelterScene")
            {
                if (StatusSystem.Instance != null)
                {
                    // StatusSystem.Instance.SetMinusDurability(1f);
                    StatusSystem.Instance.SetMinusOxygen(1f);
                    // StatusSystem.Instance.SetMinusEnergy(1f);
               
                    // 디버그 로그 (필요시 주석 처리)
                    // Debug.Log($"내구도: {(int)StatusSystem.Instance.GetDurability()}");
                    Debug.Log($"산소: {(int)StatusSystem.Instance.GetOxygen()}");
                    // Debug.Log($"에너지: {(int)StatusSystem.Instance.GetEnergy()}");
               
                    // 게임 오버 체크
                    CheckGameOver();
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 코루틴 정리
        if (statusDecreaseCoroutine != null)
        {
            StopCoroutine(statusDecreaseCoroutine);
            statusDecreaseCoroutine = null;
        }
    }

    private void CheckGameOver()
    {
        if (StatusSystem.Instance == null) return;
        
        double oxygen = StatusSystem.Instance.GetOxygen();
        double energy = StatusSystem.Instance.GetEnergy();
        double durability = StatusSystem.Instance.GetDurability();
        
        // 하나라도 0 이하가 되면 게임 오버
        if (oxygen <= 0 || energy <= 0 || durability <= 0)
        {
            Debug.Log("게임 오버!");
            
            // 게임 오버 처리
            Pause();
            
            // TODO: 게임 오버 씬으로 이동
            // SceneSystem.Instance.LoadGameOverScene();
            SceneSystem.Instance.LoadTitleScene();
        }
    }
    
    // 씬 전환 시 스택 초기화
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 씬이 로드되면 스택 초기화
        activeMenuStack.Clear();
        Resume(); // 시간 정상화
        Debug.Log($"GameSystem: 씬 로드됨 - {scene.name}, 메뉴 스택 초기화");
    }
}