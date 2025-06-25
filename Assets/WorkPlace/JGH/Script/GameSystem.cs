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
    
    void Update()
    {
        // ESC 키 입력 체크
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC 키 눌림");
            
            // MenuSystem이 존재하는지 확인
            if (MenuSystem.Instance == null)
            {
                Debug.LogError("MenuSystem.Instance가 null입니다!");
                return;
            }
            
            HandleEscapeKey();
        }
        
        // 메뉴 상태 변화 감지 (스택 동기화)
        UpdateMenuStack();
    }
    
    private void UpdateMenuStack()
    {
        if (MenuSystem.Instance == null) return;
        
        GameObject mainMenu = MenuSystem.Instance.MainMenu;
        GameObject settingMenu = MenuSystem.Instance.SettingMenu;
        GameObject exitMenu = MenuSystem.Instance.ExitMenu;
        
        // 현재 활성화된 메뉴 확인
        GameObject currentActiveMenu = null;
        
        if (settingMenu != null && settingMenu.activeSelf)
            currentActiveMenu = settingMenu;
        else if (exitMenu != null && exitMenu.activeSelf)
            currentActiveMenu = exitMenu;
        else if (mainMenu != null && mainMenu.activeSelf)
            currentActiveMenu = mainMenu;
        
        // 스택의 최상단과 현재 활성 메뉴가 다르면 업데이트
        if (currentActiveMenu != null)
        {
            if (activeMenuStack.Count == 0 || activeMenuStack.Peek() != currentActiveMenu)
            {
                // 메인 메뉴에서 서브 메뉴로 전환된 경우
                if (currentActiveMenu != mainMenu && mainMenu != null && !mainMenu.activeSelf)
                {
                    // 메인 메뉴가 스택에 없으면 추가
                    if (!activeMenuStack.Contains(mainMenu))
                    {
                        activeMenuStack.Push(mainMenu);
                        Debug.Log($"스택에 추가: {mainMenu.name}");
                    }
                }
                
                // 현재 메뉴를 스택에 추가
                if (!activeMenuStack.Contains(currentActiveMenu))
                {
                    activeMenuStack.Push(currentActiveMenu);
                    Debug.Log($"스택에 추가: {currentActiveMenu.name}");
                }
            }
        }
    }
    
    private void HandleEscapeKey()
    {
        // 현재 씬이 타이틀 씬인지 확인 (타이틀에서는 ESC 작동 안함)
        if (SceneSystem.Instance != null && SceneSystem.Instance.GetCurrentSceneName() == "TitleScene")
        {
            Debug.Log("타이틀 씬에서는 ESC가 작동하지 않습니다.");
            return;
        }

        GameObject mainMenu = MenuSystem.Instance.MainMenu;
        GameObject settingMenu = MenuSystem.Instance.SettingMenu;
        GameObject exitMenu = MenuSystem.Instance.ExitMenu;

        // 스택에 메뉴가 있는 경우
        if (activeMenuStack.Count > 0)
        {
            GameObject currentMenu = activeMenuStack.Pop();
            currentMenu.SetActive(false);
            Debug.Log($"스택에서 제거하고 비활성화: {currentMenu.name}");
            
            // 이전 메뉴가 있으면 활성화
            if (activeMenuStack.Count > 0)
            {
                GameObject previousMenu = activeMenuStack.Peek();
                previousMenu.SetActive(true);
                Debug.Log($"이전 메뉴 활성화: {previousMenu.name}");
            }
            else
            {
                // 모든 메뉴가 닫혔으므로 게임 재개
                Resume();
                Debug.Log("모든 메뉴 닫고 게임 재개");
            }
        }
        else
        {
            // 스택이 비어있으면 메인 메뉴 열기
            if (mainMenu != null)
            {
                mainMenu.SetActive(true);
                activeMenuStack.Push(mainMenu);
                Pause();
                Debug.Log("메인 메뉴 열림");
            }
        }
    }
    
    void Pause()
    {
        Time.timeScale = 0f;  // 게임 시간 정지
        isPaused = true;
        Debug.Log("게임 일시정지");
    }

    public void Resume()
    {
        Time.timeScale = 1f;  // 게임 시간 재개
        isPaused = false;
        Debug.Log("게임 재개");
    }
    
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
            string currentScene = SceneSystem.Instance.GetCurrentSceneName();
            if (currentScene == "TitleScene") continue;
       
            // 게임 씬(쉘터, 탐험 등)에서만 스테이터스 감소
            if (currentScene == "DevShelterScene" || currentScene == "Test" || 
                currentScene == "ShelterScene" || currentScene == "FarmingScene")
            {
                StatusSystem.Instance.SetMinusDurability(1f);
                StatusSystem.Instance.SetMinusOxygen(1f);
                StatusSystem.Instance.SetMinusEnergy(1f);
           
                // 디버그 로그 (필요시 주석 처리)
                Debug.Log($"내구도: {(int)StatusSystem.Instance.GetDurability()}");
                Debug.Log($"산소: {(int)StatusSystem.Instance.GetOxygen()}");
                Debug.Log($"에너지: {(int)StatusSystem.Instance.GetEnergy()}");
           
                // 게임 오버 체크
                CheckGameOver();
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
        double oxygen = StatusSystem.Instance.GetOxygen();
        double energy = StatusSystem.Instance.GetEnergy();
        double durability = StatusSystem.Instance.GetDurability();
        
        // 하나라도 0 이하가 되면 게임 오버
        if (oxygen <= 0 || energy <= 0 || durability <= 0)
        {
            Debug.Log("게임 오버!");
            
            // 게임 오버 처리
            Time.timeScale = 0f;
            
            // TODO: 게임 오버 씬으로 이동
            // SceneSystem.Instance.LoadGameOverScene();
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
        Debug.Log($"씬 로드됨: {scene.name}, 메뉴 스택 초기화");
    }
}