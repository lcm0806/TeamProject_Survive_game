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
    
    [Header("Scene Settings")]
    [SerializeField] private Object _titleScene;
    [SerializeField] private Object _shelterScene;
    [SerializeField] private Object _farmingScene;
    [SerializeField] private Object _dayTransitionScene;
    
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
        LoadScene(_titleScene);
    }
    
    /// <summary>
    /// 쉘터 씬으로 이동
    /// </summary>
    public void LoadShelterScene()
    {
        LoadScene(_shelterScene);
    }
    
    /// <summary>
    /// 농장 씬으로 이동
    /// </summary>
    public void LoadFarmingScene()
    {
        LoadScene(_farmingScene);
    }
    
    /// <summary>
    /// 다음날 전환 씬으로 이동
    /// </summary>
    public void LoadDayTransitionScene()
    {
        LoadScene(_dayTransitionScene);
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
        
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
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
}