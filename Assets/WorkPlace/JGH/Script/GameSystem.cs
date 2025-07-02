using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class GameSystem : Singleton<GameSystem>
{
    private bool isPaused = false;
    private Stack<GameObject> activeMenuStack = new Stack<GameObject>(); // 활성화된 메뉴들을 추적하는 스택
    private Coroutine statusDecreaseCoroutine;
    
    // 디버깅용 플래그
    private bool enableDebugLogs = true;

    void Awake()
    {
        SingletonInit();
    }
    
    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void Pause()
    {
        Time.timeScale = 0f;  // 게임 시간 정지
        isPaused = true;
        
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 게임 일시정지");
        
        // 파밍 씬에서 일시 정지 시 마우스 잠김 문제
        if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void Resume()
    {
        Time.timeScale = 1f;  // 게임 시간 재개
        isPaused = false;
        
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 게임 재개");

        // 현재 씬에 맞는 커서 상태 설정
        SetCursorStateForCurrentScene();
    }
    
    /// <summary>
    /// 현재 씬에 맞는 커서 상태 설정
    /// </summary>
    private void SetCursorStateForCurrentScene()
    {
        string currentScene = SceneSystem.Instance?.GetCurrentSceneName() ?? "";
        
        if (enableDebugLogs)
            Debug.Log($"[GameSystem] 커서 상태 설정 - 현재 씬: {currentScene}");
        
        if (currentScene == SceneSystem.Instance?.GetFarmingSceneName())
        {
            // 파밍 씬
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (currentScene == "TitleScene" || currentScene == "MainMenuScene")
        {
            // 메뉴 씬
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 기타 게임 씬 (대부분의 게임 씬에서는 커서가 보여야 함)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
        
        if (enableDebugLogs)
            Debug.Log($"[GameSystem] 커서 설정 완료 - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }
    
    /// <summary>
    /// 환경설정 메뉴 진입 시 호출
    /// </summary>
    public void OnEnterSettingsMenu()
    {
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 환경설정 메뉴 진입");
        
        // 게임이 실행 중이면 일시정지
        if (!isPaused)
        {
            Pause();
        }
        
        // 커서 상태 강제 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// 환경설정 메뉴 종료 시 호출
    /// </summary>
    public void OnExitSettingsMenu()
    {
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 환경설정 메뉴 종료");
        
        // 게임 상태 강제 검증 및 복원
        ValidateAndRestoreGameState();
        
        // 약간의 딜레이 후 상태 재검증
        StartCoroutine(DelayedStateValidation());
    }
    
    /// <summary>
    /// 딜레이된 상태 검증 (환경설정 종료 후 안정화를 위해)
    /// </summary>
    private IEnumerator DelayedStateValidation()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        
        ValidateAndRestoreGameState();
        
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 딜레이된 상태 검증 완료");
    }
    
    /// <summary>
    /// 강제 게임 재개 (안전장치)
    /// </summary>
    public void ForceResume()
    {
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 강제 게임 재개 시작");
        
        // Time.timeScale 강제 복원
        Time.timeScale = 1f;
        isPaused = false;
        
        // 스택 초기화
        activeMenuStack.Clear();
        
        // 현재 씬에 맞는 커서 상태 설정
        SetCursorStateForCurrentScene();
        
        if (enableDebugLogs)
            Debug.Log($"[GameSystem] 강제 게임 재개 완료 - Time.timeScale: {Time.timeScale}");
    }
    
    /// <summary>
    /// 게임 상태 검증 및 복원
    /// </summary>
    public void ValidateAndRestoreGameState()
    {
        if (enableDebugLogs)
            Debug.Log("[GameSystem] 게임 상태 검증 시작");
        
        string currentScene = SceneSystem.Instance?.GetCurrentSceneName() ?? "";
        
        // 타이틀 씬이나 메뉴 씬에서는 항상 재개 상태여야 함
        if (currentScene == "TitleScene" || currentScene == "MainMenuScene")
        {
            if (isPaused || Time.timeScale != 1f)
            {
                if (enableDebugLogs)
                    Debug.Log("[GameSystem] 메뉴 씬에서 게임 상태 복원");
                
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
        
        // Time.timeScale 검증
        if (Time.timeScale != 1f && !isPaused)
        {
            Debug.LogWarning($"[GameSystem] Time.timeScale이 비정상적입니다: {Time.timeScale}, 강제 복원합니다.");
            Time.timeScale = 1f;
        }
        
        // 일시정지 상태와 Time.timeScale 불일치 해결
        if (isPaused && Time.timeScale == 1f)
        {
            Debug.LogWarning("[GameSystem] 일시정지 상태이지만 Time.timeScale이 1입니다. 상태를 동기화합니다.");
            isPaused = false;
        }
        
        if (!isPaused && Time.timeScale == 0f)
        {
            Debug.LogWarning("[GameSystem] 게임이 일시정지되지 않았지만 Time.timeScale이 0입니다. 복원합니다.");
            Time.timeScale = 1f;
        }
        
        // 커서 상태 복원
        SetCursorStateForCurrentScene();
        
        if (enableDebugLogs)
            Debug.Log($"[GameSystem] 게임 상태 검증 완료 - isPaused: {isPaused}, Time.timeScale: {Time.timeScale}");
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
            MenuSystem.Instance.ShowGameoverView();
        }
    }

    void GameOver()
    {
        
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
        
        if (enableDebugLogs)
            Debug.Log($"[GameSystem] 씬 로드됨 - {scene.name}, 메뉴 스택 초기화");
    }
    
    // ========== 디버깅 메서드들 ==========
    
    [ContextMenu("Debug - Print Game State")]
    void DebugPrintGameState()
    {
        Debug.Log("=== Game System State ===");
        Debug.Log($"isPaused: {isPaused}");
        Debug.Log($"Time.timeScale: {Time.timeScale}");
        Debug.Log($"activeMenuStack.Count: {activeMenuStack.Count}");
        Debug.Log($"Cursor.visible: {Cursor.visible}");
        Debug.Log($"Cursor.lockState: {Cursor.lockState}");
        Debug.Log($"Current Scene: {SceneSystem.Instance?.GetCurrentSceneName()}");
        Debug.Log("========================");
    }
    
    [ContextMenu("Debug - Force Validate State")]
    void DebugForceValidateState()
    {
        ValidateAndRestoreGameState();
        Debug.Log("[GameSystem] 강제 상태 검증 완료");
    }
    
    // 게임 상태 프로퍼티들 (외부에서 확인용)
    public bool IsPaused => isPaused;
    public int ActiveMenuCount => activeMenuStack.Count;
}