using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                }
            }
            return _instance;
        }
    }
    
    [Header("Scene Settings")]
    [SerializeField] private SceneAsset _titleScene;
    [SerializeField] private SceneAsset _shelterScene;
    [SerializeField] private SceneAsset _farmingScene;
    [SerializeField] private SceneAsset _dayTransitionScene;
    
    // 메인 씬
    public string TitleSceneName => GetSceneName(_titleScene);
    // 쉘터 씬
    public string ShelterSceneName => GetSceneName(_shelterScene);
    // 파밍 씬
    public string FarmingSceneName => GetSceneName(_farmingScene);
    // 다음날 씬
    public string DayTransitionSceneName => GetSceneName(_dayTransitionScene);
    
    private void Awake()
    {
        // 싱글톤 중복 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
    /// <summary>
    /// 타이틀 씬으로 이동
    /// </summary>
    public void LoadTitleScene()
    {
        LoadSceneIfValid(_titleScene);
    }
    
    /// <summary>
    /// 쉘터 씬으로 이동
    /// </summary>
    public void LoadShelterScene()
    {
        LoadSceneIfValid(_shelterScene);
    }
    
    /// <summary>
    /// 농장 씬으로 이동
    /// </summary>
    public void LoadFarmingScene()
    {
        LoadSceneIfValid(_farmingScene);
    }
    
    /// <summary>
    /// 다음날 전환 씬으로 이동
    /// </summary>
    public void LoadDayTransitionScene()
    {
        LoadSceneIfValid(_dayTransitionScene);
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
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// SceneAsset에서 씬 이름을 안전하게 가져오기
    /// </summary>
    /// <param name="sceneAsset">씬 에셋</param>
    /// <returns>씬 이름 또는 빈 문자열</returns>
    private string GetSceneName(SceneAsset sceneAsset)
    {
        return sceneAsset != null ? sceneAsset.name : string.Empty;
    }
    
    /// <summary>
    /// SceneAsset이 유효한 경우에만 씬을 로드
    /// </summary>
    /// <param name="sceneAsset">로드할 씬 에셋</param>
    private void LoadSceneIfValid(SceneAsset sceneAsset)
    {
        if (sceneAsset == null)
        {
            Debug.LogError("Scene asset is not assigned!");
            return;
        }
        
        SceneManager.LoadScene(sceneAsset.name);
    }
   
}