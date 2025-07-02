using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using DesignPattern;

public class SceneSystem : Singleton<SceneSystem>
{
    [Header("Scene Names - 빌드 설정에서 추가된 씬 이름들")] 
    [SerializeField] private string _titleSceneName;
    [SerializeField] private string _shelterSceneName;
    [SerializeField] private string _farmingSceneName;
    [SerializeField] private string _dayTransitionSceneName;
    
    
    /// <summary>
    /// 타이틀 씬 이름을 반환합니다.
    /// </summary>
    /// <returns>타이틀 씬 이름</returns>
    public string GetTitleSceneName()
    {
        return _titleSceneName;
    }

    /// <summary>
    /// 쉘터 씬 이름을 반환합니다.
    /// </summary>
    /// <returns>쉘터 씬 이름</returns>
    public string GetShelterSceneName()
    {
        return _shelterSceneName;
    }

    /// <summary>
    /// 파밍 씬 이름을 반환합니다.
    /// </summary>
    /// <returns>파밍 씬 이름</returns>
    public string GetFarmingSceneName()
    {
        return _farmingSceneName;
    }

    /// <summary>
    /// 하루 전환 씬 이름을 반환합니다.
    /// </summary>
    /// <returns>하루 전환 씬 이름</returns>
    public string GetDayTransitionSceneName()
    {
        return _dayTransitionSceneName;
    }
    
    
    private void Awake()
    {
        // Singleton 초기화 먼저 호출
        SingletonInit();
        
        ValidateSceneNames();
        
    }
    
    /// <summary>
    /// 씬 이름들이 제대로 설정되었는지 검증합니다.
    /// </summary>
    private void ValidateSceneNames()
    {
        bool hasError = false;
    
        if (string.IsNullOrEmpty(_titleSceneName))
        {
            Debug.LogError("Title Scene Name이 설정되지 않았습니다!");
            hasError = true;
        }
    
        if (string.IsNullOrEmpty(_shelterSceneName))
        {
            Debug.LogError("Shelter Scene Name이 설정되지 않았습니다!");
            hasError = true;
        }
    
        if (string.IsNullOrEmpty(_farmingSceneName))
        {
            Debug.LogError("Farming Scene Name이 설정되지 않았습니다!");
            hasError = true;
        }
    
        if (string.IsNullOrEmpty(_dayTransitionSceneName))
        {
            Debug.LogError("Day Transition Scene Name이 설정되지 않았습니다!");
            hasError = true;
        }
    
        if (hasError)
        {
            Debug.LogError("SceneSystem: 일부 씬 이름이 설정되지 않았습니다. Inspector에서 확인해주세요!");
        }
    }
    
    
    /// <summary>
    /// 타이틀 씬으로 이동
    /// </summary>
    public void LoadTitleScene()
    {
        LoadSceneWithDelay(_titleSceneName);
    }
    
    /// <summary>
    /// 쉘터 씬으로 이동
    /// </summary>
    public void LoadShelterScene()
    {
        // 씬 로드 후 저장하도록 변경
        LoadSceneWithDelay(_shelterSceneName);
    }
    
    /// <summary>
    /// 농장 씬으로 이동
    /// </summary>
    public void LoadFarmingScene()
    {
        if (StatusSystem.Instance.GetIsToDay() == true)
        {
            return;
        }
        // 탐색 여부 먼저 설정
        StatusSystem.Instance.SetIsToDay(true);
        // 씬 로드는 저장 없이
        LoadSceneWithDelay(_farmingSceneName);
    }
    
    
    /// <summary>
    /// 씬 이름으로 직접 로드 (딜레이 포함)
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadSceneWithDelay(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    /// <summary>
    /// 씬 로드 후 자동 저장 (딜레이 포함)
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadSceneWithDelayAndSave(string sceneName)
    {
        StartCoroutine(LoadSceneAndSaveCoroutine(sceneName));
    }

    /// <summary>
    /// 씬 이름으로 직접 로드
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty!");
            return;
        }
        
        // 씬이 빌드 설정에 포함되어 있는지 확인
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' is not in build settings or doesn't exist!");
            
            // 빌드 설정의 모든 씬 출력
            Debug.Log("Build Settings에 포함된 씬들:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"  [{i}] {sceneNameInBuild}");
            }
        }
    }
    
    /// <summary>
    /// Object로 씬 로드
    /// </summary>
    /// <param name="sceneObject">로드할 씬 오브젝트</param>
    public void LoadScene(Object sceneObject)
    {
        if (sceneObject == null)
        {
            Debug.LogError("Scene object is null!");
            return;
        }
        
        LoadScene(sceneObject.name);
    }
    
    /// <summary>
    /// 씬 로드 코루틴 (약간의 딜레이 추가)
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Debug.Log($"씬 로드 준비: {sceneName}");
        
        // 짧은 딜레이 (UI 업데이트 등을 위해)
        yield return new WaitForSeconds(0.1f);
        
        LoadScene(sceneName);
    }
    
    /// <summary>
    /// 씬 로드 후 자동 저장 코루틴
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    /// <returns></returns>
    private IEnumerator LoadSceneAndSaveCoroutine(string sceneName)
    {
        Debug.Log($"씬 로드 및 자동 저장 준비: {sceneName}");
        
        // 씬 로드 시작
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        
        // 씬 로드 완료 대기
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        // 씬이 완전히 로드된 후 한 프레임 더 대기
        yield return new WaitForEndOfFrame();
        
        // 씬 로드 완료 확인
        Debug.Log($"씬 로드 완료: {SceneManager.GetActiveScene().name}");
        
        // 이제 저장 실행
        if (FileSystem.Instance != null)
        {
            GameData data = new GameData
            {
                currentDay = StatusSystem.Instance.GetCurrentDay(),
                oxygenRemaining = StatusSystem.Instance.GetOxygen(),
                electricalEnergy = StatusSystem.Instance.GetEnergy(),
                shelterDurability = StatusSystem.Instance.GetDurability(),
                isToDay = StatusSystem.Instance.GetIsToDay()
            };
            FileSystem.Instance.SaveGameData(data);
            
            Debug.Log($"씬 '{sceneName}' 로드 후 자동 저장 완료");
        }
        else
        {
            Debug.LogWarning("FileSystem.Instance가 null이어서 자동 저장을 건너뜁니다.");
        }
    }
    
    /// <summary>
    /// 현재 씬 이름 가져오기
    /// </summary>
    /// <returns></returns>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// 특정 씬 로드 완료 후 콜백 실행
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    /// <param name="onComplete">완료 후 실행할 콜백</param>
    public void LoadSceneWithCallback(string sceneName, Action onComplete)
    {
        StartCoroutine(LoadSceneWithCallbackCoroutine(sceneName, onComplete));
    }
    
    private IEnumerator LoadSceneWithCallbackCoroutine(string sceneName, Action onComplete)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForEndOfFrame();
        
        onComplete?.Invoke();
    }
}