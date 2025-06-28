using UnityEngine;
using System;
using System.IO;
using System.Collections;

// ========== 데이터 클래스들 ==========

[System.Serializable]
public class GameSaveData
{
    [SerializeField] public string saveName = "Manual_Save";
    [SerializeField] public string saveDate;
    [SerializeField] public string currentSceneName = "";
    [SerializeField] public float totalPlayTime;
    [SerializeField] public PlayerStatusData playerStatus;

    public GameSaveData()
    {
        playerStatus = new PlayerStatusData();
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public DateTime GetSaveDateTime()
    {
        if (DateTime.TryParse(saveDate, out DateTime result)) return result;
        return DateTime.Now;
    }
}

[System.Serializable]
public class PlayerStatusData
{
    public int currentDay = 1;
    public double oxygenRemaining = 100.0;
    public double electricalEnergy = 100.0;
    public double shelterDurability = 100.0;
    public bool isToDay = true;
    public Vector3 playerPosition = Vector3.zero;
    public Vector3 playerRotation = Vector3.zero;

    public PlayerStatusData()
    {
        if (StatusSystem.Instance != null)
        {
            currentDay = StatusSystem.Instance.GetCurrentDay();
            oxygenRemaining = StatusSystem.Instance.GetOxygen();
            electricalEnergy = StatusSystem.Instance.GetEnergy();
            shelterDurability = StatusSystem.Instance.GetDurability();
            isToDay = StatusSystem.Instance.GetIsToDay();
        }
    }
}

[System.Serializable]
public class GameSettingsData
{
    public int qualityLevel = 2;
    public bool fullScreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public float mouseSensitivity = 1.0f;
    public bool autoSave = true;
}

// ========== 파일 시스템 ==========

public class FileSystem : MonoBehaviour
{
    private static FileSystem _instance;
    public static FileSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FileSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("FileSystem");
                    _instance = go.AddComponent<FileSystem>();
                }
            }
            return _instance;
        }
    }

    [Header("저장 설정")]
    [SerializeField] private bool enableManualSave = false;
    [SerializeField] private bool enableEncryption = false;

    private string saveDirectory;
    private string gameDataFileName = "gamedata.json";
    private string settingsFileName = "settings.json";
    private string encryptionKey = "OdsodBNcoWIBDia288dkxdgjJsdo2jJKJFdifd822321";

    private float gameStartTime;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Awake에서 초기화를 수행하여 다른 어떤 스크립트보다 먼저 실행되도록 보장합니다.
        InitializeFileSystem();
    }

    void Start()
    {
        // 게임 시작 시간 기록 및 설정 불러오기는 Start에서 수행합니다.
        gameStartTime = Time.time;
        LoadGameSettings();
    }

    void InitializeFileSystem()
    {
        try
        {
            saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                Debug.Log($"Save directory created at: {saveDirectory}");
            }
            else
            {
                Debug.Log($"Save directory found at: {saveDirectory}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize save directory: {e.Message}");
            saveDirectory = null;
        }
    }
    
    /// <summary>
    /// (이전 이름: SaveCreateGameData) 게임 데이터를 생성하거나 덮어씁니다.
    /// </summary>
    public void SaveOrUpdateGameData(string saveName = "Auto_Save")
    {
        Debug.Log($"[저장시작] SaveOrUpdateGameData 호출 - 저장명: {saveName}");
        
        try
        {
            // 저장 전 현재 상태 출력
            Debug.Log($"[저장시작] 현재 씬: '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'");
            Debug.Log($"[저장시작] 현재 시간: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            
            GameSaveData saveData = CreateGameSaveData(saveName);
            
            // 생성된 데이터 검증
            if (saveData == null)
            {
                Debug.LogError("[저장시작] 저장 데이터 생성 실패");
                return;
            }
            
            if (string.IsNullOrEmpty(saveData.currentSceneName))
            {
                Debug.LogError("[저장시작] 씬 이름이 비어있음!");
                // 강제로 현재 씬 이름 설정
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                saveData.currentSceneName = activeScene.name;
                Debug.LogWarning($"[저장시작] 씬 이름 강제 설정: '{saveData.currentSceneName}'");
            }
            
            bool result = SaveGameDataToFile(saveData);
            
            if (result)
            {
                Debug.Log($"[저장완료] 게임 데이터 저장/업데이트 완료: {saveName}");
                Debug.Log($"[저장완료] 저장된 씬: '{saveData.currentSceneName}'");
            }
            else
            {
                Debug.LogError("[저장완료] 게임 데이터 저장/업데이트 실패");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[저장오류] 게임 데이터 저장/업데이트 실패: {e.Message}");
            Debug.LogError($"[저장오류] 스택 트레이스: {e.StackTrace}");
        }
    }

    /// <summary>
    /// 게임 데이터 불러오기
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            Debug.Log("[로딩] 게임 데이터 로드 시작");
            
            GameSaveData loadedData = LoadGameDataFromFile();
            if (loadedData != null)
            {
                Debug.Log($"[로딩] 데이터 로드 성공");
                Debug.Log($"[로딩] 저장 이름: {loadedData.saveName}");
                Debug.Log($"[로딩] 저장 날짜: {loadedData.saveDate}");
                Debug.Log($"[로딩] 저장된 씬: {loadedData.currentSceneName}");
                Debug.Log($"[로딩] 플레이 시간: {loadedData.totalPlayTime}초");
                
                ApplyGameData(loadedData);
                Debug.Log($"게임 데이터 로드 완료: {loadedData.saveName}");
                return true;
            }
            else
            {
                Debug.LogWarning("저장된 게임 데이터가 없거나 로드에 실패했습니다.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 로드 실패: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 게임 데이터 존재 여부 확인
    /// </summary>
    public bool HasGameData()
    {
        string filePath = Path.Combine(saveDirectory, gameDataFileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// 게임 데이터 삭제
    /// </summary>
    public bool DeleteGameData()
    {
        try
        {
            string filePath = Path.Combine(saveDirectory, gameDataFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("게임 데이터 삭제 완료");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 삭제 실패: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 메뉴에서 게임 불러오기 시도 (MenuSystem에서 호출)
    /// </summary>
    public void TryLoadGameFromMenu()
    {
        Debug.Log("=== 메뉴에서 게임 로드 시도 ===");
        
        // 1. 파일 시스템 초기화 확인
        if (!IsInitialized())
        {
            Debug.Log("FileSystem 강제 초기화");
            ForceInitialize();
        }
        
        // 2. 저장 파일 존재 확인
        if (HasGameData())
        {
            Debug.Log("저장된 게임 데이터 발견");
            
            // 3. 로드 전 현재 상태 기록
            Debug.Log($"로드 전 현재 씬: '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'");
            
            // 저장된 데이터 미리 확인
            GameSaveData testData = LoadGameDataFromFile();
            if (testData != null)
            {
                Debug.Log($"저장된 씬: '{testData.currentSceneName}'");
                Debug.Log($"현재 씬: '{GetCurrentSceneName()}'");
                
                // 씬이 빌드에 있는지 확인
                bool canLoad = Application.CanStreamedLevelBeLoaded(testData.currentSceneName);
                Debug.Log($"씬 로드 가능: {canLoad}");
                
                if (!canLoad)
                {
                    Debug.LogError($"씬 '{testData.currentSceneName}'이 빌드에 없습니다!");
                }
            }
            
            // 4. 게임 로드
            bool loaded = LoadGame();
            
            if (loaded)
            {
                Debug.Log("게임 로드 성공");
                if (MenuSystem.Instance != null)
                {
                    MenuSystem.Instance.OnLoadGameSuccess();
                }
            }
            else
            {
                Debug.LogError("게임 로드 실패");
                if (MenuSystem.Instance != null)
                {
                    MenuSystem.Instance.ShowWarningDialog("게임 불러오기에 실패했습니다!");
                }
            }
        }
        else
        {
            Debug.Log("저장된 게임 데이터 없음");
            if (MenuSystem.Instance != null)
            {
                MenuSystem.Instance.ShowWarningDialog("저장된 게임 데이터가 없습니다!");
            }
        }
        
        Debug.Log("=== 메뉴에서 게임 로드 시도 종료 ===");
    }

    /// <summary>
    /// 게임 저장 데이터 객체 생성 (강화된 버전)
    /// </summary>
    private GameSaveData CreateGameSaveData(string saveName)
    {
        Debug.Log($"[저장] 게임 데이터 생성 시작 - 저장명: {saveName}");
        
        GameSaveData saveData = new GameSaveData();
        saveData.saveName = string.IsNullOrEmpty(saveName) ? "Game_Save" : saveName;
        saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        if (gameStartTime <= 0) gameStartTime = Time.time;
        saveData.totalPlayTime = Time.time - gameStartTime;
        
        // 현재 씬 이름 저장 (강화된 로직)
        string currentScene = GetCurrentSceneName();
        saveData.currentSceneName = currentScene;
        
        Debug.Log($"[저장] 저장할 씬 이름: '{saveData.currentSceneName}'");
        Debug.Log($"[저장] 저장 날짜: {saveData.saveDate}");
        Debug.Log($"[저장] 플레이 시간: {saveData.totalPlayTime}초");
        
        // 씬 이름이 제대로 설정되었는지 검증
        if (string.IsNullOrEmpty(saveData.currentSceneName) || 
            saveData.currentSceneName == "UnknownScene" || 
            saveData.currentSceneName == "ErrorScene")
        {
            Debug.LogWarning($"[저장] 씬 이름이 이상합니다: '{saveData.currentSceneName}'. 강제로 현재 활성 씬으로 설정합니다.");
            
            // 강제로 Unity SceneManager에서 다시 가져오기
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                saveData.currentSceneName = activeScene.name;
                Debug.Log($"[저장] 강제 수정된 씬 이름: '{saveData.currentSceneName}'");
            }
        }
        
        SavePlayerDataToSaveData(saveData);
        
        Debug.Log($"[저장] 게임 데이터 생성 완료");
        return saveData;
    }

    /// <summary>
    /// 게임 데이터를 파일에 저장 (검증 추가)
    /// </summary>
    private bool SaveGameDataToFile(GameSaveData saveData)
    {
        Debug.Log($"[저장] 파일 저장 시작");
        Debug.Log($"[저장] 저장할 데이터 - 씬: '{saveData.currentSceneName}', 이름: '{saveData.saveName}'");
        
        bool result = SaveToFile(saveData, gameDataFileName);
        
        if (result)
        {
            Debug.Log($"[저장] 파일 저장 성공");
            
            // 저장 후 검증: 다시 읽어서 제대로 저장되었는지 확인
            GameSaveData verificationData = LoadGameDataFromFile();
            if (verificationData != null)
            {
                Debug.Log($"[저장검증] 저장된 씬 이름: '{verificationData.currentSceneName}'");
                if (verificationData.currentSceneName != saveData.currentSceneName)
                {
                    Debug.LogError($"[저장검증] 씬 이름 불일치! 저장시도: '{saveData.currentSceneName}', 실제저장: '{verificationData.currentSceneName}'");
                }
                else
                {
                    Debug.Log($"[저장검증] 씬 이름 저장 성공 확인");
                }
            }
            else
            {
                Debug.LogError($"[저장검증] 저장 후 재읽기 실패");
            }
        }
        else
        {
            Debug.LogError($"[저장] 파일 저장 실패");
        }
        
        return result;
    }

    /// <summary>
    /// 파일에서 게임 데이터 불러오기
    /// </summary>
    private GameSaveData LoadGameDataFromFile()
    {
        return LoadFromFile<GameSaveData>(gameDataFileName);
    }

    /// <summary>
    /// 플레이어 데이터를 저장 데이터 객체에 저장
    /// </summary>
    private void SavePlayerDataToSaveData(GameSaveData saveData)
    {
        if (StatusSystem.Instance != null)
        {
            saveData.playerStatus.currentDay = StatusSystem.Instance.GetCurrentDay();
            saveData.playerStatus.oxygenRemaining = StatusSystem.Instance.GetOxygen();
            saveData.playerStatus.electricalEnergy = StatusSystem.Instance.GetEnergy();
            saveData.playerStatus.shelterDurability = StatusSystem.Instance.GetDurability();
            saveData.playerStatus.isToDay = StatusSystem.Instance.GetIsToDay();
        }
        
        GameObject player = FindPlayerObject();
        if (player != null)
        {
            saveData.playerStatus.playerPosition = player.transform.position;
            saveData.playerStatus.playerRotation = player.transform.eulerAngles;
        }
    }

    /// <summary>
    /// 불러온 게임 데이터 적용 (수정된 버전)
    /// </summary>
    private void ApplyGameData(GameSaveData loadedData)
    {
        Debug.Log($"[로딩] 게임 데이터 적용 시작");
        Debug.Log($"[로딩] 저장된 씬 이름: {loadedData.currentSceneName}");
        Debug.Log($"[로딩] 현재 씬 이름: {GetCurrentSceneName()}");
        
        // 먼저 플레이어 데이터 복원
        RestorePlayerData(loadedData);
        
        // 씬 로딩은 별도로 처리 (현재 씬과 다른 경우에만)
        if (!string.IsNullOrEmpty(loadedData.currentSceneName))
        {
            string currentScene = GetCurrentSceneName();
            if (!string.Equals(currentScene, loadedData.currentSceneName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"[로딩] 씬 전환: {currentScene} -> {loadedData.currentSceneName}");
                StartCoroutine(LoadSceneAfterDataRestore(loadedData.currentSceneName));
            }
            else
            {
                Debug.Log($"[로딩] 동일한 씬이므로 씬 전환 생략");
            }
        }
        else
        {
            Debug.LogWarning("[로딩] 저장된 씬 이름이 없습니다!");
        }
    }

    /// <summary>
    /// 데이터 복원 후 씬 로딩을 위한 코루틴
    /// </summary>
    private IEnumerator LoadSceneAfterDataRestore(string sceneName)
    {
        // 약간의 딜레이를 두어 데이터 복원이 완료되도록 함
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[로딩] 씬 로딩 시도: {sceneName}");
        
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("[로딩] SceneSystem.Instance가 null입니다!");
            // 직접 씬 로딩 시도
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// 플레이어 데이터 복원
    /// </summary>
    private void RestorePlayerData(GameSaveData loadedData)
    {
        if (loadedData.playerStatus == null) return;
        
        if (StatusSystem.Instance != null)
        {
            RestoreStatusValue("Oxygen", StatusSystem.Instance.GetOxygen(), loadedData.playerStatus.oxygenRemaining);
            RestoreStatusValue("Energy", StatusSystem.Instance.GetEnergy(), loadedData.playerStatus.electricalEnergy);
            RestoreStatusValue("Durability", StatusSystem.Instance.GetDurability(), loadedData.playerStatus.shelterDurability);
            StatusSystem.Instance.SetIsToDay(loadedData.playerStatus.isToDay);
        }
        
        GameObject player = FindPlayerObject();
        if (player != null)
        {
            player.transform.position = loadedData.playerStatus.playerPosition;
            player.transform.eulerAngles = loadedData.playerStatus.playerRotation;
        }
    }

    /// <summary>
    /// 스테이터스 값 복원
    /// </summary>
    private void RestoreStatusValue(string statusType, double currentValue, double targetValue)
    {
        double difference = targetValue - currentValue;
        if (Math.Abs(difference) < 0.01 || StatusSystem.Instance == null) return;
        
        switch (statusType)
        {
            case "Oxygen":
                if (difference > 0) StatusSystem.Instance.SetPlusOxygen(difference);
                else StatusSystem.Instance.SetMinusOxygen(-difference);
                break;
            case "Energy":
                if (difference > 0) StatusSystem.Instance.SetPlusEnergy(difference);
                else StatusSystem.Instance.SetMinusEnergy(-difference);
                break;
            case "Durability":
                if (difference > 0) StatusSystem.Instance.SetPlusDurability(difference);
                else StatusSystem.Instance.SetMinusDurability(-difference);
                break;
        }
    }

    /// <summary>
    /// 파일에 데이터 저장
    /// </summary>
    private bool SaveToFile<T>(T data, string fileName)
    {
        try
        {
            // saveDirectory가 초기화되지 않았다면 다시 초기화 시도
            if (string.IsNullOrEmpty(saveDirectory))
            {
                Debug.LogWarning("Save Directory not initialized, attempting to reinitialize...");
                InitializeFileSystem();
                
                // 재초기화 후에도 여전히 문제가 있다면 저장 중단
                if (string.IsNullOrEmpty(saveDirectory))
                {
                    Debug.LogError("Save Directory initialization failed!");
                    return false;
                }
            }
            
            string filePath = Path.Combine(saveDirectory, fileName);
            string directory = Path.GetDirectoryName(filePath);
            
            if (!Directory.Exists(directory)) 
                Directory.CreateDirectory(directory);
            
            string json = JsonUtility.ToJson(data, true);
            if (string.IsNullOrEmpty(json)) return false;
            
            if (enableEncryption) 
                json = EncryptString(json, encryptionKey);
            
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 저장 실패: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 파일에서 데이터 불러오기
    /// </summary>
    private T LoadFromFile<T>(string fileName) where T : class
    {
        try
        {
            // saveDirectory가 초기화되지 않았다면 다시 초기화 시도
            if (string.IsNullOrEmpty(saveDirectory))
            {
                Debug.LogWarning("Save Directory not initialized, attempting to reinitialize...");
                InitializeFileSystem();
                
                if (string.IsNullOrEmpty(saveDirectory))
                {
                    Debug.LogError("Save Directory initialization failed!");
                    return null;
                }
            }

            string filePath = Path.Combine(saveDirectory, fileName);
            if (!File.Exists(filePath)) return null;
            
            string json = File.ReadAllText(filePath);
            if (enableEncryption) 
                json = DecryptString(json, encryptionKey);
            
            T result = JsonUtility.FromJson<T>(json);
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 로드 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 게임 설정 저장
    /// </summary>
    public void SaveGameSettings()
    {
        if (!EnsureDirectoryInitialized())
        {
            Debug.LogWarning("Cannot save game settings: Directory not initialized");
            return;
        }
        
        GameSettingsData settings = new GameSettingsData();
        SaveGameSettingsToData(settings);
        bool result = SaveSettingsDataToFile(settings);
        
        if (result)
        {
            Debug.Log("게임 설정 저장 완료");
        }
        else
        {
            Debug.LogError("게임 설정 저장 실패");
        }
    }

    /// <summary>
    /// 게임 설정 불러오기
    /// </summary>
    public void LoadGameSettings()
    {
        GameSettingsData settings = LoadSettingsDataFromFile();
        if (settings != null)
        {
            ApplyGameSettings(settings);
            Debug.Log("게임 설정 로드 완료");
        }
        else
        {
            Debug.Log("게임 설정 파일이 없습니다. 기본 설정을 사용합니다.");
        }
    }

    /// <summary>
    /// 설정 데이터를 파일에 저장
    /// </summary>
    private bool SaveSettingsDataToFile(GameSettingsData settings)
    {
        return SaveToFile(settings, settingsFileName);
    }

    /// <summary>
    /// 파일에서 설정 데이터 불러오기
    /// </summary>
    private GameSettingsData LoadSettingsDataFromFile()
    {
        return LoadFromFile<GameSettingsData>(settingsFileName);
    }

    /// <summary>
    /// 게임 설정 데이터에 현재 설정 저장
    /// </summary>
    private void SaveGameSettingsToData(GameSettingsData settings)
    {
        settings.qualityLevel = QualitySettings.GetQualityLevel();
        settings.fullScreen = Screen.fullScreen;
        settings.resolutionWidth = Screen.currentResolution.width;
        settings.resolutionHeight = Screen.currentResolution.height;
    }

    /// <summary>
    /// 게임 설정 적용
    /// </summary>
    private void ApplyGameSettings(GameSettingsData settings)
    {
        QualitySettings.SetQualityLevel(settings.qualityLevel);
        Screen.fullScreen = settings.fullScreen;
    }

    /// <summary>
    /// 플레이어 오브젝트 찾기
    /// </summary>
    private GameObject FindPlayerObject()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player ?? GameObject.Find("Player");
    }

    /// <summary>
    /// 현재 씬 이름 가져오기 (강화된 버전)
    /// </summary>
    private string GetCurrentSceneName()
    {
        string sceneName = "";
        
        try
        {
            // 방법 1: Unity SceneManager에서 직접 가져오기
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid() && !string.IsNullOrEmpty(activeScene.name))
            {
                sceneName = activeScene.name;
                Debug.Log($"[씬이름] Unity SceneManager에서 가져옴: '{sceneName}'");
            }
            
            // 방법 2: SceneSystem이 있으면 추가 확인
            if (SceneSystem.Instance != null)
            {
                string sceneSystemName = SceneSystem.Instance.GetCurrentSceneName();
                if (!string.IsNullOrEmpty(sceneSystemName))
                {
                    // 두 값이 다르면 경고
                    if (!string.IsNullOrEmpty(sceneName) && sceneName != sceneSystemName)
                    {
                        Debug.LogWarning($"[씬이름] SceneManager와 SceneSystem 불일치: '{sceneName}' vs '{sceneSystemName}'");
                    }
                    sceneName = sceneSystemName; // SceneSystem 우선
                    Debug.Log($"[씬이름] SceneSystem에서 가져옴: '{sceneName}'");
                }
            }
            
            // 방법 3: 빈 문자열이면 기본값 설정
            if (string.IsNullOrEmpty(sceneName))
            {
                sceneName = "UnknownScene";
                Debug.LogWarning($"[씬이름] 씬 이름을 가져올 수 없어서 기본값 사용: '{sceneName}'");
            }
            
            Debug.Log($"[씬이름] 최종 결정된 씬 이름: '{sceneName}'");
            return sceneName;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[씬이름] 씬 이름 가져오기 실패: {e.Message}");
            return "ErrorScene";
        }
    }

    /// <summary>
    /// 문자열 암호화
    /// </summary>
    private string EncryptString(string text, string key)
    {
        string result = "";
        for (int i = 0; i < text.Length; i++)
            result += (char)(text[i] ^ key[i % key.Length]);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result));
    }

    /// <summary>
    /// 문자열 복호화
    /// </summary>
    private string DecryptString(string encryptedText, string key)
    {
        try
        {
            byte[] data = Convert.FromBase64String(encryptedText);
            string text = System.Text.Encoding.UTF8.GetString(data);
            string result = "";
            for (int i = 0; i < text.Length; i++)
                result += (char)(text[i] ^ key[i % key.Length]);
            return result;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 안전한 초기화 확인을 위한 헬퍼 메서드
    /// </summary>
    private bool EnsureDirectoryInitialized()
    {
        if (string.IsNullOrEmpty(saveDirectory))
        {
            InitializeFileSystem();
        }
        return !string.IsNullOrEmpty(saveDirectory);
    }

    /// <summary>
    /// 초기화 상태 확인
    /// </summary>
    public bool IsInitialized()
    {
        return !string.IsNullOrEmpty(saveDirectory);
    }

    /// <summary>
    /// 강제 초기화
    /// </summary>
    public void ForceInitialize()
    {
        if (!IsInitialized())
        {
            InitializeFileSystem();
        }
    }

    /// <summary>
    /// 현재 gamedata.json 파일 내용 출력 (디버깅용)
    /// </summary>
    public void PrintCurrentGameDataFile()
    {
        try
        {
            if (!EnsureDirectoryInitialized()) return;
            
            string filePath = Path.Combine(saveDirectory, gameDataFileName);
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                Debug.Log($"[파일내용] gamedata.json 내용:\n{jsonContent}");
                
                // JSON 파싱해서 씬 이름만 따로 출력
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(jsonContent);
                if (data != null)
                {
                    Debug.Log($"[파일내용] 파싱된 씬 이름: '{data.currentSceneName}'");
                }
            }
            else
            {
                Debug.Log("[파일내용] gamedata.json 파일이 존재하지 않음");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[파일내용] 파일 읽기 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 저장된 게임 데이터 정보 확인 (디버깅용)
    /// </summary>
    public void PrintSavedGameInfo()
    {
        try
        {
            GameSaveData loadedData = LoadGameDataFromFile();
            if (loadedData != null)
            {
                Debug.Log("=== 저장된 게임 데이터 정보 ===");
                Debug.Log($"저장 이름: {loadedData.saveName}");
                Debug.Log($"저장 날짜: {loadedData.saveDate}");
                Debug.Log($"저장된 씬: {loadedData.currentSceneName}");
                Debug.Log($"플레이 시간: {loadedData.totalPlayTime}초");
                Debug.Log($"현재 날짜: {loadedData.playerStatus?.currentDay}");
                Debug.Log($"산소: {loadedData.playerStatus?.oxygenRemaining}");
                Debug.Log($"에너지: {loadedData.playerStatus?.electricalEnergy}");
                Debug.Log($"내구도: {loadedData.playerStatus?.shelterDurability}");
                Debug.Log($"낮/밤: {(loadedData.playerStatus?.isToDay == true ? "낮" : "밤")}");
                Debug.Log($"플레이어 위치: {loadedData.playerStatus?.playerPosition}");
                Debug.Log("================================");
            }
            else
            {
                Debug.Log("저장된 게임 데이터가 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"저장된 게임 데이터 정보 확인 실패: {e.Message}");
        }
    }

    // ========== Unity 이벤트 ==========
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGameSettings();
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGameSettings();
    }
    
    void OnDestroy()
    {
        // 인스턴스가 정상적으로 초기화된 경우에만 설정 저장
        if (!string.IsNullOrEmpty(saveDirectory) && _instance == this)
        {
            SaveGameSettings();
        }
    }
}