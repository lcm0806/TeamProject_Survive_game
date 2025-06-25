using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSystem : MonoBehaviour
{
    private static SceneSystem _instance;
    public static SceneSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SceneSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SceneSystem");
                    _instance = go.AddComponent<SceneSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Scene Names - 빌드 설정에서 추가된 씬 이름들")]
    [SerializeField] private string _titleSceneName = "TitleScene";
    [SerializeField] private string _shelterSceneName = "DevShelterScene";
    [SerializeField] private string _farmingSceneName = "Test";
    [SerializeField] private string _dayTransitionSceneName = "DevShelterScene";
    
    private void Awake()
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
    /// 타이틀 씬으로 이동
    /// </summary>
    public void LoadTitleScene()
    {
        // LoadScene(_titleScene);
        LoadSceneWithDelay(_titleSceneName);
    }
    
    /// <summary>
    /// 쉘터 씬으로 이동
    /// </summary>
    public void LoadShelterScene()
    {
        // LoadScene(_shelterScene);
        LoadSceneWithDelay(_shelterSceneName);
    }
    
    /// <summary>
    /// 농장 씬으로 이동
    /// </summary>
    public void LoadFarmingScene()
    {
        // LoadScene(_farmingScene);
        LoadSceneWithDelay(_farmingSceneName);
        // 탐색 여부
        StatusSystem.Instance.SetIsToDay(true);
    }
    
    /// <summary>
    /// 다음날 전환 씬으로 이동
    /// </summary>
    public void LoadDayTransitionScene()
    {
        // LoadScene(_dayTransitionScene);
        LoadSceneWithDelay(_dayTransitionSceneName);
        // 날짜 + 1
        StatusSystem.Instance.NextCurrentDay();
        // 탐색 여부
        StatusSystem.Instance.SetIsToDay(false);
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
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Debug.Log($"씬 로드 준비: {sceneName}");
        
        // 짧은 딜레이 (UI 업데이트 등을 위해)
        yield return new WaitForSeconds(0.1f);
        
        LoadScene(sceneName);
    }
    
    /// <summary>
    /// 현재 씬 이름 가져오기
    /// </summary>
    /// <returns></returns>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}