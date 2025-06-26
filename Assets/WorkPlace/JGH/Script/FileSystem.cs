using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

// ========== 데이터 클래스들 ==========

[System.Serializable]
public class GameSaveData
{
    public string saveName = "새 게임";
    public string saveDate;
    public string currentSceneName;
    public float totalPlayTime;
    public PlayerStatusData playerStatus;
    public GameSettingsData gameSettings;

    public GameSaveData()
    {
        playerStatus = new PlayerStatusData();
        gameSettings = new GameSettingsData();
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

[System.Serializable]
public class SaveSlotInfo
{
    public bool isEmpty;
    public string saveName;
    public string saveDate;
    public int currentDay;
    public float totalPlayTime;
    public string currentSceneName;
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
    [SerializeField] private int maxSaveSlots = 5;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f;
    [SerializeField] private bool enableEncryption = false;

    private string saveDirectory;
    private string autoSaveFileName = "autosave.json";
    private string settingsFileName = "settings.json";
    private string encryptionKey = "YourGameSecretKey2024";

    private float lastAutoSaveTime;
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
        gameStartTime = Time.time;
        lastAutoSaveTime = Time.time;
        LoadGameSettings();
    }

    void Update()
    {
        if (enableAutoSave && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            AutoSave();
            lastAutoSaveTime = Time.time;
        }
    }

    void InitializeFileSystem()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);
    }

    public bool SaveGame(int slotIndex, string saveName = "")
    {
        try
        {
            string path = GetSaveFilePath(slotIndex);
            SaveData saveData = new SaveData
            {
                saveName = saveName,
                saveDate = DateTime.Now.ToString(),
                currentDay = StatusSystem.Instance.GetCurrentDay(),
                totalPlayTime = Time.realtimeSinceStartup,
                
                oxygen = StatusSystem.Instance.GetOxygen(),
                energy = StatusSystem.Instance.GetEnergy(),
                durability = StatusSystem.Instance.GetDurability(),
                isToDay = StatusSystem.Instance.GetIsToDay(),
                currentSceneName = SceneSystem.Instance.GetCurrentSceneName()
            };

            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(path, jsonData);
            
            Debug.Log($"게임 저장 완료 - 슬롯: {slotIndex}, 경로: {path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
            return false;
        }
    }
    
    [Serializable]
    public class SaveData
    {
        public string saveName;
        public string saveDate;
        public int currentDay;
        public float totalPlayTime;
    
        public double oxygen;
        public double energy;
        public double durability;
        public bool isToDay;
        public string currentSceneName;
    }

    public bool LoadGame(int slotIndex)
    {
        try
        {
            string path = GetSaveFilePath(slotIndex);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"저장 파일이 없음: {path}");
                return false;
            }

            string jsonData = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

            StatusSystem.Instance.SetMinusOxygen(100 - saveData.oxygen);
            StatusSystem.Instance.SetMinusEnergy(100 - saveData.energy);
            StatusSystem.Instance.SetMinusDurability(100 - saveData.durability);
            StatusSystem.Instance.SetIsToDay(saveData.isToDay);
            
            SceneSystem.Instance.LoadScene(saveData.currentSceneName);
            
            Debug.Log($"게임 로드 완료 - 슬롯: {slotIndex}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"로드 실패: {e.Message}");
            return false;
        }
    }

    public void AutoSave()
    {
        if (ShouldSkipAutoSave()) return;
        GameSaveData saveData = CreateSaveData("자동 저장");
        SaveToFile(saveData, autoSaveFileName);
    }

    public bool LoadAutoSave()
    {
        GameSaveData loadedData = LoadFromFile<GameSaveData>(autoSaveFileName);
        if (loadedData != null)
        {
            ApplyLoadedData(loadedData);
            return true;
        }
        return false;
    }

    private GameSaveData CreateSaveData(string saveName)
    {
        GameSaveData saveData = new GameSaveData();
        saveData.saveName = string.IsNullOrEmpty(saveName) ? "게임 저장" : saveName;
        saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (gameStartTime <= 0) gameStartTime = Time.time;
        saveData.totalPlayTime = Time.time - gameStartTime;
        saveData.currentSceneName = GetCurrentSceneName();
        SavePlayerData(saveData);
        SaveGameSettingsToData(saveData.gameSettings);
        return saveData;
    }

    private void SavePlayerData(GameSaveData saveData)
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

    private void ApplyLoadedData(GameSaveData loadedData)
    {
        RestorePlayerData(loadedData);
        if (loadedData.gameSettings != null)
            ApplyGameSettings(loadedData.gameSettings);
    }

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

    private bool SaveToFile<T>(T data, string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(saveDirectory)) return false;
            string filePath = Path.Combine(saveDirectory, fileName);
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            string json = JsonUtility.ToJson(data, true);
            if (string.IsNullOrEmpty(json)) return false;
            if (enableEncryption) json = EncryptString(json, encryptionKey);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch { return false; }
    }

    private T LoadFromFile<T>(string fileName) where T : class
    {
        try
        {
            string filePath = Path.Combine(saveDirectory, fileName);
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            if (enableEncryption) json = DecryptString(json, encryptionKey);
            T result = JsonUtility.FromJson<T>(json);
            return result;
        }
        catch { return null; }
    }

    private void SaveSlotInfo(int slotIndex, GameSaveData saveData)
    {
        SaveSlotInfo slotInfo = new SaveSlotInfo
        {
            isEmpty = false,
            saveName = saveData.saveName,
            saveDate = saveData.saveDate,
            currentDay = saveData.playerStatus.currentDay,
            currentSceneName = saveData.currentSceneName,
            totalPlayTime = saveData.totalPlayTime
        };
        SaveToFile(slotInfo, $"slot_info_{slotIndex}.json");
    }

    public SaveSlotInfo[] GetAllSlotInfo()
    {
        SaveSlotInfo[] infos = new SaveSlotInfo[5];
        
        for (int i = 0; i < 5; i++)
        {
            string path = GetSaveFilePath(i);
            infos[i] = new SaveSlotInfo();
            
            if (File.Exists(path))
            {
                try
                {
                    string jsonData = File.ReadAllText(path);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                    
                    infos[i].isEmpty = false;
                    infos[i].saveName = saveData.saveName;
                    infos[i].saveDate = saveData.saveDate;
                    infos[i].currentDay = saveData.currentDay;
                    infos[i].totalPlayTime = saveData.totalPlayTime;
                }
                catch
                {
                    infos[i].isEmpty = true;
                }
            }
            else
            {
                infos[i].isEmpty = true;
            }
        }
        
        return infos;
    }
    
    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"savedata_{slotIndex}.json");
    }

    public bool DeleteSaveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots) return false;
        try
        {
            string path = GetSaveFilePath(slotIndex);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"저장 파일 삭제 완료: {path}");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"삭제 실패: {e.Message}");
            return false;
        }
    }

    public void SaveGameSettings()
    {
        GameSettingsData settings = new GameSettingsData();
        SaveGameSettingsToData(settings);
        SaveToFile(settings, settingsFileName);
    }

    public void LoadGameSettings()
    {
        GameSettingsData settings = LoadFromFile<GameSettingsData>(settingsFileName);
        if (settings != null)
            ApplyGameSettings(settings);
    }

    private void SaveGameSettingsToData(GameSettingsData settings)
    {
        settings.qualityLevel = QualitySettings.GetQualityLevel();
        settings.fullScreen = Screen.fullScreen;
        settings.resolutionWidth = Screen.currentResolution.width;
        settings.resolutionHeight = Screen.currentResolution.height;
    }

    private void ApplyGameSettings(GameSettingsData settings)
    {
        QualitySettings.SetQualityLevel(settings.qualityLevel);
        Screen.fullScreen = settings.fullScreen;
    }

    private GameObject FindPlayerObject()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player ?? GameObject.Find("Player");
    }

    private string GetCurrentSceneName()
    {
        return SceneSystem.Instance?.GetCurrentSceneName() ?? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private bool ShouldSkipAutoSave()
    {
        string currentScene = GetCurrentSceneName();
        if (IsStartScene(currentScene)) return true;
        if (GameSystem.Instance != null && GameSystem.Instance.IsPaused()) return true;
        return false;
    }

    private bool IsStartScene(string sceneName)
    {
        string[] startScenes = {
            "StartScene", "TitleScene", "MainMenuScene", "MenuScene",
            "Intro", "MainMenu"
        };
        foreach (string startScene in startScenes)
        {
            if (sceneName.Equals(startScene, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private void DeleteFileIfExists(string fileName)
    {
        string filePath = Path.Combine(saveDirectory, fileName);
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private string EncryptString(string text, string key)
    {
        string result = "";
        for (int i = 0; i < text.Length; i++)
            result += (char)(text[i] ^ key[i % key.Length]);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result));
    }

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

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && enableAutoSave) AutoSave();
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && enableAutoSave) AutoSave();
    }
    void OnDestroy()
    {
        SaveGameSettings();
    }
}