using UnityEngine;
using System;
using System.IO;

// ========== 데이터 클래스들 ==========

[System.Serializable]
public class GameSaveData
{
    public string saveName = "Manual_Save";
    public string saveDate;
    public string currentSceneName;
    public float totalPlayTime;
    public PlayerStatusData playerStatus;

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
                    DontDestroyOnLoad(go);
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
        
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializeFileSystem();
        gameStartTime = Time.time;
        LoadGameSettings();
    }

    void InitializeFileSystem()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);
    }

    /// <summary>
    /// 게임 데이터 저장 (수동 저장)
    /// </summary>
    public bool SaveGame(string saveName = "Manual_Save")
    {
        if (!enableManualSave)
        {
            Debug.LogWarning("수동 저장이 비활성화되어 있습니다.");
            return false;
        }

        try
        {
            GameSaveData saveData = CreateGameSaveData(saveName);
            bool result = SaveGameDataToFile(saveData);
            
            if (result)
            {
                Debug.Log($"게임 데이터 저장 완료: {saveName}");
            }
            else
            {
                Debug.LogError("게임 데이터 저장 실패");
            }
            
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 저장 실패: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 게임 데이터 불러오기
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            GameSaveData loadedData = LoadGameDataFromFile();
            if (loadedData != null)
            {
                ApplyGameData(loadedData);
                Debug.Log($"게임 데이터 로드 완료: {loadedData.saveName}");
                return true;
            }
            else
            {
                Debug.LogWarning("저장된 게임 데이터가 없습니다.");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 로드 실패: {e.Message}");
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
        if (HasGameData())
        {
            Debug.Log("게임 데이터 발견 - 게임 불러오기");
            bool loaded = LoadGame();
            
            if (loaded)
            {
                // MenuSystem에 성공 알림
                if (MenuSystem.Instance != null)
                {
                    MenuSystem.Instance.OnLoadGameSuccess();
                }
            }
            else
            {
                if (MenuSystem.Instance != null)
                {
                    MenuSystem.Instance.ShowWarningDialog("게임 불러오기에 실패했습니다!");
                }
            }
        }
        else
        {
            Debug.Log("게임 데이터 없음 - 경고창 표시");
            if (MenuSystem.Instance != null)
            {
                MenuSystem.Instance.ShowWarningDialog("저장된 게임 데이터가 없습니다!");
            }
        }
    }

    /// <summary>
    /// 게임 저장 데이터 생성 (게임 데이터만)
    /// </summary>
    private GameSaveData CreateGameSaveData(string saveName)
    {
        GameSaveData saveData = new GameSaveData();
        saveData.saveName = string.IsNullOrEmpty(saveName) ? "Game_Save" : saveName;
        saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        if (gameStartTime <= 0) gameStartTime = Time.time;
        saveData.totalPlayTime = Time.time - gameStartTime;
        saveData.currentSceneName = GetCurrentSceneName();
        
        SavePlayerDataToSaveData(saveData);
        
        return saveData;
    }

    /// <summary>
    /// 게임 데이터를 파일에 저장
    /// </summary>
    private bool SaveGameDataToFile(GameSaveData saveData)
    {
        return SaveToFile(saveData, gameDataFileName);
    }

    /// <summary>
    /// 파일에서 게임 데이터 불러오기
    /// </summary>
    private GameSaveData LoadGameDataFromFile()
    {
        return LoadFromFile<GameSaveData>(gameDataFileName);
    }

    /// <summary>
    /// 플레이어 데이터를 저장 데이터에 저장
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
    /// 게임 데이터 적용 (게임 데이터만)
    /// </summary>
    private void ApplyGameData(GameSaveData loadedData)
    {
        RestorePlayerData(loadedData);
            
        // 씬 이동
        if (!string.IsNullOrEmpty(loadedData.currentSceneName))
        {
            SceneSystem.Instance?.LoadScene(loadedData.currentSceneName);
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
            if (string.IsNullOrEmpty(saveDirectory)) return false;
            
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
    /// 현재 씬 이름 가져오기
    /// </summary>
    private string GetCurrentSceneName()
    {
        return SceneSystem.Instance?.GetCurrentSceneName() ?? 
               UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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
        SaveGameSettings();
    }
}