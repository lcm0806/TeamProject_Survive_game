using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using DesignPattern;


// ========== 데이터 클래스들 ==========
[System.Serializable]
public class GameData
{
    public int currentDay;
    public double oxygenRemaining;
    public double electricalEnergy;
    public double shelterDurability;
    public bool isToDay;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public int quantity;
    public int slotIndex;
}

[System.Serializable]
public class InventoryData
{
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public List<InventoryItemData> hotbarItems = new List<InventoryItemData>();
    public int currentHotbarSlotIndex = 0;
}

[System.Serializable]
public class StorageData
{
    public List<InventoryItemData> storageItems = new List<InventoryItemData>();
}

[System.Serializable]
public class ItemData
{
    public InventoryData inventoryData;
    public StorageData storageData;
}

public class FileSystem : Singleton<FileSystem>
{
    private string settingPath;
    private string gameDataPath;
    private string itemDataPath;
    private ItemData pendingItemData;
    private bool shouldLoadItemsOnStart = false;
    
      private bool isInitialized = false;
      
    void Awake()
    {
     if (Instance != null && Instance != this)
        {
            Debug.Log("FileSystem 인스턴스가 이미 존재합니다. 중복 제거.");
            Destroy(gameObject);
            return;
        }
        SingletonInit();
        
        
        // 플랫폼별 최적 경로 설정
        string dataDirectory = GetPlatformDataDirectory();
        
        // 데이터 디렉토리 생성
        if (EnsureDirectoryExists(dataDirectory))
        {
            // 파일 경로 설정
            settingPath = Path.Combine(dataDirectory, "setting.json");
            gameDataPath = Path.Combine(dataDirectory, "gamedata.json");
            itemDataPath = Path.Combine(dataDirectory, "item.json");
            
            // 🔥 핵심 수정 3: 초기화 성공 표시
            isInitialized = true;
            
            // 디버그 정보 출력
            LogPlatformInfo(dataDirectory);
        }
        else
        {
            Debug.LogError("FileSystem 초기화 실패!");
            isInitialized = false;
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    
    void Start()
    {
        if (shouldLoadItemsOnStart)
        {
            shouldLoadItemsOnStart = false;
            LoadAndApplyItemData();
        }
        
        StartCoroutine(CheckForInstancesAndApplyPendingData());
    }
    
    // ========== 플랫폼별 경로 설정 ==========
    private string GetPlatformDataDirectory()
    {
        // 플랫폼과 관계없이 일관된 경로 사용
        string baseDirectory = Application.persistentDataPath;

        return baseDirectory;
    }
    
    private bool EnsureDirectoryExists(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"데이터 디렉토리 생성: {directoryPath}");
            }
            
            // 쓰기 권한 테스트
            return TestDirectoryPermissions(directoryPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"디렉토리 생성/확인 실패: {e.Message}");
            Debug.LogError($"경로: {directoryPath}");
            
            // 플랫폼별 권한 안내
            ShowPlatformPermissionGuide();
            return false;
        }
    }
    
    private bool TestDirectoryPermissions(string directoryPath)
    {
        try
        {
            string testFile = Path.Combine(directoryPath, "permission_test.tmp");
            File.WriteAllText(testFile, "permission test");
            
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
                Debug.Log($"디렉토리 쓰기 권한 확인 완료: {directoryPath}");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"디렉토리 쓰기 권한 없음: {e.Message}");
            ShowPlatformPermissionGuide();
            return false;
        }
    }
    
    private void ShowPlatformPermissionGuide()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXPlayer:
                Debug.LogWarning("=== macOS 권한 설정 가이드 ===");
                Debug.LogWarning("1. 시스템 환경설정 > 보안 및 개인 정보 보호");
                Debug.LogWarning("2. 개인 정보 보호 탭 > 전체 디스크 접근 권한");
                Debug.LogWarning("3. 게임 실행 파일 추가");
                Debug.LogWarning("또는 터미널에서: chmod +x [게임경로]");
                break;
                
            case RuntimePlatform.WindowsPlayer:
                Debug.LogWarning("=== Windows 권한 설정 가이드 ===");
                Debug.LogWarning("1. 게임을 관리자 권한으로 실행");
                Debug.LogWarning("2. 바이러스 백신에서 게임 폴더 예외 처리");
                Debug.LogWarning("3. Windows Defender에서 폴더 접근 제어 확인");
                break;
                
            case RuntimePlatform.LinuxPlayer:
                Debug.LogWarning("=== Linux 권한 설정 가이드 ===");
                Debug.LogWarning("터미널에서: chmod +x [게임경로]");
                Debug.LogWarning("또는: sudo chown -R $USER:$USER ~/.local/share/");
                break;
        }
    }
    
    private void LogPlatformInfo(string dataDirectory)
    {
        Debug.Log($"=== 플랫폼 정보 ===");
        Debug.Log($"현재 플랫폼: {Application.platform}");
        Debug.Log($"데이터 저장 디렉토리: {dataDirectory}");
        Debug.Log($"설정 파일 경로: {settingPath}");
        Debug.Log($"게임 데이터 경로: {gameDataPath}");
        Debug.Log($"아이템 데이터 경로: {itemDataPath}");
        Debug.Log($"==================");
    }
    
    // ========== 설정 파일 관리 ==========
    public void SaveSetting(SettingData data)
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        try
        {
            string json = JsonUtility.ToJson(data, true);
            
            string tempPath = settingPath + ".tmp";
            File.WriteAllText(tempPath, json);
            
            if (File.Exists(settingPath))
            {
                File.Delete(settingPath);
            }
            File.Move(tempPath, settingPath);
            
            Debug.Log($"설정 저장 완료: {settingPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 저장 실패: {e.Message}");
        }
    }

    public SettingData LoadSetting()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return GetDefaultSetting();
        }
        
        try
        {
            if (File.Exists(settingPath))
            {
                string json = File.ReadAllText(settingPath);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("설정 파일이 비어있음, 기본값 반환");
                    return GetDefaultSetting();
                }
                
                SettingData data = JsonUtility.FromJson<SettingData>(json);
                Debug.Log($"설정 불러오기 완료: {settingPath}");
                return data ?? GetDefaultSetting();
            }
            else
            {
                Debug.Log("설정 파일 없음, 기본값 반환");
                return GetDefaultSetting();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"설정 불러오기 실패: {e.Message}");
            return GetDefaultSetting();
        }
    }

    private SettingData GetDefaultSetting()
    {
        return new SettingData
        {
            fullscreen = false,
            quality = 1,
            bgmVolume = 1f,
            sfxVolume = 1f
        };
    }
    
    // ========== 게임 데이터 관리 ==========
    public void SaveGameData(GameData data)
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        try
        {
            string json = JsonUtility.ToJson(data, true);
            
            string tempPath = gameDataPath + ".tmp";
            File.WriteAllText(tempPath, json);
            
            if (File.Exists(gameDataPath))
            {
                File.Delete(gameDataPath);
            }
            File.Move(tempPath, gameDataPath);
            
            Debug.Log($"게임 데이터 저장 완료: {gameDataPath}");
            
            // 아이템 데이터도 함께 저장
            SaveItemData();
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 저장 실패: {e.Message}");
        }
    }

    public GameData LoadGameData()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return GetDefaultGameData();
        }
        
        try
        {
            if (File.Exists(gameDataPath))
            {
                string json = File.ReadAllText(gameDataPath);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("게임 데이터 파일이 비어있음, 기본값 반환");
                    return GetDefaultGameData();
                }
                
                GameData data = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"게임 데이터 불러오기 완료: {gameDataPath}");
                return data ?? GetDefaultGameData();
            }
            else
            {
                Debug.Log("게임 데이터 파일 없음, 기본값 반환");
                return GetDefaultGameData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 불러오기 실패: {e.Message}");
            return GetDefaultGameData();
        }
    }

    private GameData GetDefaultGameData()
    {
        return new GameData
        {
            currentDay = 1,
            oxygenRemaining = 100f,
            electricalEnergy = 100f,
            shelterDurability = 100f,
            isToDay = true
        };
    }
    
    public void ApplyGameData(GameData data)
    {
        Debug.Log("게임 데이터 적용 시작");
        
        StartCoroutine(WaitForStatusSystemAndApply(data));
    }
    
    private System.Collections.IEnumerator WaitForStatusSystemAndApply(GameData data)
    {
        float waitTime = 0f;
        float maxWaitTime = 15f;
        
        while (StatusSystem.Instance == null && waitTime < maxWaitTime)
        {
            Debug.Log($"StatusSystem 인스턴스 대기 중... ({waitTime:F1}초)");
            yield return new UnityEngine.WaitForSeconds(0.5f);
            waitTime += 0.5f;
        }
        
        if (StatusSystem.Instance == null)
        {
            Debug.LogError("StatusSystem 인스턴스를 찾을 수 없습니다!");
            yield break;
        }
        
        try
        {
            StatusSystem.Instance.SetCurrentDay(data.currentDay);
            StatusSystem.Instance.SetOxygen(data.oxygenRemaining);
            StatusSystem.Instance.SetEnergy(data.electricalEnergy);
            StatusSystem.Instance.SetDurability(data.shelterDurability);
            StatusSystem.Instance.SetIsToDay(data.isToDay);
            
            Debug.Log("StatusSystem 데이터 적용 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"StatusSystem 데이터 적용 실패: {e.Message}");
        }
        
        LoadAndApplyItemData();
        
        if (MenuSystem.Instance != null && MenuSystem.Instance.MainMenu != null)
        {
            MenuSystem.Instance.MainMenu.SetActive(false);
        }
    }
    
    public void LoadGame()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        Debug.Log("게임 로드 시작");
        
        try
        {
            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null)
            {
                inputManager.enabled = false;
                Debug.Log("InputManager 임시 비활성화");
            }
            
            GameData gameData = LoadGameData();
            
            bool hasItemData = File.Exists(itemDataPath);
            Debug.Log($"아이템 데이터 파일 존재: {hasItemData}");
            
            if (hasItemData)
            {
                shouldLoadItemsOnStart = true;
            }
            
            // 🔥 핵심 수정 10: 안전한 게임 데이터 적용
            ApplyGameData(gameData);
            
            Debug.Log("게임 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 로드 실패: {e.Message}");
        }
    }
    
    // ========== 아이템 데이터 관리 ==========
    public void SaveItemData()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        try
        {
            ItemData itemData = new ItemData
            {
                inventoryData = CollectInventoryData(),
                storageData = CollectStorageData()
            };
            
            string json = JsonUtility.ToJson(itemData, true);
            
            string tempPath = itemDataPath + ".tmp";
            File.WriteAllText(tempPath, json);
            
            if (File.Exists(itemDataPath))
            {
                File.Delete(itemDataPath);
            }
            File.Move(tempPath, itemDataPath);
            
            // 저장 검증
            if (File.Exists(itemDataPath))
            {
                FileInfo fileInfo = new FileInfo(itemDataPath);
                Debug.Log($"아이템 데이터 저장 완료: {itemDataPath} (크기: {fileInfo.Length} bytes)");
                
                // 내용 검증
                string savedContent = File.ReadAllText(itemDataPath);
                if (savedContent.Length > 0)
                {
                    Debug.Log("아이템 데이터 검증 성공");
                }
                else
                {
                    Debug.LogError("저장된 아이템 데이터가 비어있습니다!");
                }
            }
            else
            {
                Debug.LogError("아이템 데이터 파일이 생성되지 않았습니다!");
            }
            
            DebugSavedItemData(itemData);
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 저장 실패: {e.Message}");
            Debug.LogError($"경로: {itemDataPath}");
        }
    }
    
    public void LoadAndApplyItemData()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        try
        {
            Debug.Log($"아이템 데이터 파일 확인: {itemDataPath}");
            Debug.Log($"파일 존재 여부: {File.Exists(itemDataPath)}");
            
            if (File.Exists(itemDataPath))
            {
                FileInfo fileInfo = new FileInfo(itemDataPath);
                Debug.Log($"파일 크기: {fileInfo.Length} bytes");
                Debug.Log($"파일 생성 시간: {fileInfo.CreationTime}");
                Debug.Log($"파일 수정 시간: {fileInfo.LastWriteTime}");
                
                string json = File.ReadAllText(itemDataPath);
                Debug.Log($"읽어온 JSON 길이: {json.Length}");
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("아이템 데이터 파일이 비어있습니다!");
                    return;
                }
                
                ItemData data = JsonUtility.FromJson<ItemData>(json);
                if (data == null)
                {
                    Debug.LogError("JSON 파싱 실패!");
                    return;
                }
                
                Debug.Log($"아이템 데이터 불러오기 완료: {itemDataPath}");
                DebugLoadedItemData(data);
                
                StartCoroutine(WaitForInstancesAndRestoreItems(data));
            }
            else
            {
                Debug.Log($"아이템 데이터 파일 없음: {itemDataPath}");
                
                string directory = Path.GetDirectoryName(itemDataPath);
                Debug.Log($"디렉토리 존재 여부: {Directory.Exists(directory)}");
                
                if (Directory.Exists(directory))
                {
                    string[] files = Directory.GetFiles(directory, "*.json");
                    Debug.Log($"같은 디렉토리의 JSON 파일들: {string.Join(", ", files)}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 데이터 불러오기 실패: {e.Message}");
        }
    }
    
    public void SaveItemDataOnly()
    {
        SaveItemData();
    }
    
    public void LoadItemDataOnly()
    {
        LoadAndApplyItemData();
    }
    
    public void OnSceneLoaded()
    {
        Debug.Log("씬 로드 완료, 아이템 데이터 적용 준비");
        
        // 🔥 핵심 수정 12: InputManager 재활성화 개선
        StartCoroutine(ReenableInputManagerDelayed());
        
        if (shouldLoadItemsOnStart)
        {
            shouldLoadItemsOnStart = false;
            StartCoroutine(DelayedItemLoad());
        }
    }
    
    private System.Collections.IEnumerator ReenableInputManagerDelayed()
    {
        yield return new UnityEngine.WaitForSeconds(1f);
        
        InputManager inputManager = FindObjectOfType<InputManager>();
        if (inputManager != null)
        {
            inputManager.enabled = true;
            Debug.Log("InputManager 다시 활성화");
        }
    }
    
    private System.Collections.IEnumerator DelayedItemLoad()
    {
        yield return new UnityEngine.WaitForSeconds(2f);
        Debug.Log("지연된 아이템 로드 시작");
        LoadAndApplyItemData();
    }
    
    // ========== 파일 존재 확인 메서드들 ==========
    public bool HasSaveData()
    {
        return isInitialized && File.Exists(gameDataPath);
    }
    
    public bool HasItemData()
    {
        return isInitialized && File.Exists(itemDataPath);
    }
    
    public bool HasSettingData()
    {
        return isInitialized && File.Exists(settingPath);
    }
    
    public void DeleteAllSaveData()
    {
        if (!isInitialized)
        {
            Debug.LogError("FileSystem이 초기화되지 않았습니다!");
            return;
        }
        
        try
        {
            if (File.Exists(gameDataPath))
            {
                File.Delete(gameDataPath);
                Debug.Log("게임 데이터 삭제 완료");
            }
            
            if (File.Exists(itemDataPath))
            {
                File.Delete(itemDataPath);
                Debug.Log("아이템 데이터 삭제 완료");
            }
            
            if (File.Exists(settingPath))
            {
                File.Delete(settingPath);
                Debug.Log("설정 데이터 삭제 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 데이터 삭제 실패: {e.Message}");
        }
    }
    
    // ========== 내부 메서드들 (기존과 동일) ==========
    private System.Collections.IEnumerator WaitForInstancesAndRestoreItems(ItemData data)
    {
        Debug.Log("인스턴스 대기 시작");
        
        float waitTime = 0f;
        float maxWaitTime = 20f;
        
        while ((Inventory.Instance == null || Storage.Instance == null) && waitTime < maxWaitTime)
        {
            Inventory[] allInventories = FindObjectsOfType<Inventory>();
            Storage[] allStorages = FindObjectsOfType<Storage>();
            
            if (waitTime % 1f < 0.1f)
            {
                Debug.Log($"인스턴스 대기 중... Inventory: {(Inventory.Instance != null ? "OK" : "NULL")}, Storage: {(Storage.Instance != null ? "OK" : "NULL")} ({waitTime:F1}초)");
                Debug.Log($"발견된 객체 수 - Inventory: {allInventories.Length}, Storage: {allStorages.Length}");
            }
            
            yield return new UnityEngine.WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }
        
        if (Inventory.Instance == null || Storage.Instance == null)
        {
            Debug.LogWarning($"인스턴스 대기 시간 초과! Inventory: {(Inventory.Instance != null ? "OK" : "NULL")}, Storage: {(Storage.Instance != null ? "OK" : "NULL")}");
            SavePendingItemData(data);
            Debug.Log("아이템 데이터를 임시 저장했습니다. 인스턴스가 준비되면 자동으로 적용됩니다.");
            yield break;
        }
        
        Debug.Log("인스턴스 준비 완료, 아이템 복원 시작");
        
        yield return new UnityEngine.WaitForSeconds(1f);
        yield return new UnityEngine.WaitForEndOfFrame();
        
        if (data.inventoryData != null && Inventory.Instance != null)
        {
            Debug.Log($"인벤토리 아이템 복원 시작 - 아이템 수: {data.inventoryData.inventoryItems.Count}, 핫바 아이템 수: {data.inventoryData.hotbarItems.Count}");
            
            ClearAllInventorySlots();
            yield return null;
            
            foreach (var itemData in data.inventoryData.inventoryItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    Debug.Log($"인벤토리 아이템 복원: {itemData.itemName} x{itemData.quantity} -> 슬롯 {itemData.slotIndex}");
                    RestoreItemToInventorySlot(item, itemData.quantity, itemData.slotIndex, false);
                }
                else
                {
                    Debug.LogWarning($"아이템을 찾을 수 없음: {itemData.itemName}");
                }
                yield return null;
            }
            
            foreach (var itemData in data.inventoryData.hotbarItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    Debug.Log($"핫바 아이템 복원: {itemData.itemName} x{itemData.quantity} -> 슬롯 {itemData.slotIndex}");
                    RestoreItemToInventorySlot(item, itemData.quantity, itemData.slotIndex, true);
                }
                else
                {
                    Debug.LogWarning($"아이템을 찾을 수 없음: {itemData.itemName}");
                }
                yield return null;
            }
            
            yield return null;
            Inventory.Instance.SelectHotbarSlot(data.inventoryData.currentHotbarSlotIndex);
            Debug.Log("인벤토리 아이템 복원 완료");
        }
        
        if (data.storageData != null && Storage.Instance != null)
        {
            Debug.Log($"창고 아이템 복원 시작 - 아이템 수: {data.storageData.storageItems.Count}");
            
            ClearAllStorageSlots();
            yield return null;
            
            foreach (var itemData in data.storageData.storageItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    Debug.Log($"창고 아이템 복원: {itemData.itemName} x{itemData.quantity} -> 슬롯 {itemData.slotIndex}");
                    RestoreItemToStorageSlot(item, itemData.quantity, itemData.slotIndex);
                }
                else
                {
                    Debug.LogWarning($"아이템을 찾을 수 없음: {itemData.itemName}");
                }
                yield return null;
            }
            
            Debug.Log("창고 아이템 복원 완료");
        }
        
        Debug.Log("모든 아이템 복원 작업 완료");
    }
    
    private InventoryData CollectInventoryData()
    {
        InventoryData inventoryData = new InventoryData();
        
        if (Inventory.Instance == null)
        {
            Debug.LogWarning("Inventory.Instance가 null입니다. 빈 인벤토리 데이터를 반환합니다.");
            return inventoryData;
        }
        
        var inventorySlots = GetInventorySlots();
        if (inventorySlots != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].myItemData != null)
                {
                    int quantity = inventorySlots[i].myItemUI != null ? inventorySlots[i].myItemUI.CurrentQuantity : 1;
                    inventoryData.inventoryItems.Add(new InventoryItemData
                    {
                        itemName = inventorySlots[i].myItemData.itemName,
                        quantity = quantity,
                        slotIndex = i
                    });
                }
            }
        }

        var hotbarSlots = GetHotbarSlots();
        if (hotbarSlots != null)
        {
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i].myItemData != null)
                {
                    int quantity = hotbarSlots[i].myItemUI != null ? hotbarSlots[i].myItemUI.CurrentQuantity : 1;
                    inventoryData.hotbarItems.Add(new InventoryItemData
                    {
                        itemName = hotbarSlots[i].myItemData.itemName,
                        quantity = quantity,
                        slotIndex = i
                    });
                }
            }
        }
        
        inventoryData.currentHotbarSlotIndex = Inventory.Instance._currentHotbarSlotIndex;
        return inventoryData;
    }
    
    private StorageData CollectStorageData()
    {
        StorageData storageData = new StorageData();
        
        if (Storage.Instance == null)
        {
            Debug.LogWarning("Storage.Instance가 null입니다. 빈 창고 데이터를 반환합니다.");
            return storageData;
        }
        
        var storageSlots = GetStorageSlots();
        if (storageSlots != null)
        {
            for (int i = 0; i < storageSlots.Length; i++)
            {
                if (storageSlots[i].myItemData != null)
                {
                    int quantity = storageSlots[i].myItemUI != null ? storageSlots[i].myItemUI.CurrentQuantity : 1;
                    storageData.storageItems.Add(new InventoryItemData
                    {
                        itemName = storageSlots[i].myItemData.itemName,
                        quantity = quantity,
                        slotIndex = i
                    });
                }
            }
        }
        
        return storageData;
    }

    private InventorySlot[] GetInventorySlots()
    {
        try
        {
            var field = typeof(Inventory).GetField("inventorySlots", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (InventorySlot[])field?.GetValue(Inventory.Instance);
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 슬롯 가져오기 실패: {e.Message}");
            return null;
        }
    }
    
    private InventorySlot[] GetHotbarSlots()
    {
        try
        {
            var field = typeof(Inventory).GetField("hotbarSlots", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (InventorySlot[])field?.GetValue(Inventory.Instance);
        }
        catch (Exception e)
        {
            Debug.LogError($"핫바 슬롯 가져오기 실패: {e.Message}");
            return null;
        }
    }
    
    private InventorySlot[] GetStorageSlots()
    {
        try
        {
            var field = typeof(Storage).GetField("storageSlots", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (InventorySlot[])field?.GetValue(Storage.Instance);
        }
        catch (Exception e)
        {
            Debug.LogError($"창고 슬롯 가져오기 실패: {e.Message}");
            return null;
        }
    }
    
    private Item LoadItemByName(string itemName)
    {
        try
        {
            Item[] allItems = Resources.LoadAll<Item>("Items");
            foreach (Item item in allItems)
            {
                if (item.name == itemName)
                {
                    return item;
                }
            }
            
            Item directLoad = Resources.Load<Item>($"Items/{itemName}");
            if (directLoad != null)
            {
                return directLoad;
            }
            
            Debug.LogWarning($"아이템을 찾을 수 없습니다: {itemName}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 로드 중 오류: {e.Message}");
            return null;
        }
    }
    
    private void RestoreItemToInventorySlot(Item item, int quantity, int slotIndex, bool isHotbar)
    {
        try
        {
            InventorySlot[] slots = isHotbar ? GetHotbarSlots() : GetInventorySlots();
            
            if (slots != null && slotIndex >= 0 && slotIndex < slots.Length)
            {
                var itemPrefab = GetItemPrefab();
                if (itemPrefab != null)
                {
                    var newItemUI = UnityEngine.Object.Instantiate(itemPrefab, slots[slotIndex].transform);
                    newItemUI.Initialize(item, slots[slotIndex]);
                    newItemUI.CurrentQuantity = quantity;
                    
                    if (isHotbar)
                    {
                        Inventory.Instance.CheckAndSyncSlotIfHotbar(slots[slotIndex]);
                    }
                }
                else
                {
                    slots[slotIndex].SetItemData(item);
                    slots[slotIndex].SetItemQuantity(quantity);
                    slots[slotIndex].UpdateSlotUI();
                }
            }
            else
            {
                Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}, 슬롯 배열 길이: {slots?.Length}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 아이템 복원 실패: {e.Message}");
        }
    }
    
    private void RestoreItemToStorageSlot(Item item, int quantity, int slotIndex)
    {
        try
        {
            InventorySlot[] storageSlots = GetStorageSlots();
            
            if (storageSlots != null && slotIndex >= 0 && slotIndex < storageSlots.Length)
            {
                storageSlots[slotIndex].SetItemData(item);
                storageSlots[slotIndex].SetItemQuantity(quantity);
                storageSlots[slotIndex].UpdateSlotUI();
            }
            else
            {
                Debug.LogWarning($"잘못된 창고 슬롯 인덱스: {slotIndex}, 슬롯 배열 길이: {storageSlots?.Length}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"창고 아이템 복원 실패: {e.Message}");
        }
    }
    
    private InventoryItem GetItemPrefab()
    {
        try
        {
            var field = typeof(Inventory).GetField("itemPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (InventoryItem)field?.GetValue(Inventory.Instance);
        }
        catch (Exception e)
        {
            Debug.LogError($"아이템 프리팹 가져오기 실패: {e.Message}");
            return null;
        }
    }
    
    private void ClearAllInventorySlots()
    {
        try
        {
            InventorySlot[] inventorySlots = GetInventorySlots();
            InventorySlot[] hotbarSlots = GetHotbarSlots();
            
            if (inventorySlots != null)
            {
                foreach (var slot in inventorySlots)
                {
                    slot.ClearSlot();
                }
            }
            
            if (hotbarSlots != null)
            {
                foreach (var slot in hotbarSlots)
                {
                    slot.ClearSlot();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"인벤토리 슬롯 초기화 실패: {e.Message}");
        }
    }
    
    private void ClearAllStorageSlots()
    {
        try
        {
            InventorySlot[] storageSlots = GetStorageSlots();
            
            if (storageSlots != null)
            {
                foreach (var slot in storageSlots)
                {
                    slot.ClearSlot();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"창고 슬롯 초기화 실패: {e.Message}");
        }
    }
    
    // ========== 디버그 메서드들 ==========
    private void DebugSavedItemData(ItemData data)
    {
        if (data.inventoryData != null)
        {
            Debug.Log($"저장된 인벤토리 아이템 수: {data.inventoryData.inventoryItems.Count}");
            Debug.Log($"저장된 핫바 아이템 수: {data.inventoryData.hotbarItems.Count}");
        }
        
        if (data.storageData != null)
        {
            Debug.Log($"저장된 창고 아이템 수: {data.storageData.storageItems.Count}");
        }
    }
    
    private void DebugLoadedItemData(ItemData data)
    {
        if (data.inventoryData != null)
        {
            Debug.Log($"불러온 인벤토리 아이템 수: {data.inventoryData.inventoryItems.Count}");
            foreach (var item in data.inventoryData.inventoryItems)
            {
                Debug.Log($"인벤토리 아이템: {item.itemName} x{item.quantity} @ 슬롯 {item.slotIndex}");
            }
            
            Debug.Log($"불러온 핫바 아이템 수: {data.inventoryData.hotbarItems.Count}");
            foreach (var item in data.inventoryData.hotbarItems)
            {
                Debug.Log($"핫바 아이템: {item.itemName} x{item.quantity} @ 슬롯 {item.slotIndex}");
            }
        }
        
        if (data.storageData != null)
        {
            Debug.Log($"불러온 창고 아이템 수: {data.storageData.storageItems.Count}");
            foreach (var item in data.storageData.storageItems)
            {
                Debug.Log($"창고 아이템: {item.itemName} x{item.quantity} @ 슬롯 {item.slotIndex}");
            }
        }
    }
    
    // ========== 임시 데이터 처리 메서드들 ==========
    private void SavePendingItemData(ItemData data)
    {
        pendingItemData = data;
        Debug.Log("아이템 데이터를 임시 저장했습니다.");
    }
    
    private System.Collections.IEnumerator CheckForInstancesAndApplyPendingData()
    {
        while (pendingItemData != null)
        {
            yield return new UnityEngine.WaitForSeconds(1f);
            
            if (Inventory.Instance != null && Storage.Instance != null)
            {
                Debug.Log("인스턴스가 준비되었습니다! 임시 저장된 아이템 데이터를 적용합니다.");
                yield return StartCoroutine(ApplyPendingItemData());
                break;
            }
            
            Debug.Log("인스턴스 대기 중... (임시 아이템 데이터 있음)");
        }
    }
    
    private System.Collections.IEnumerator ApplyPendingItemData()
    {
        if (pendingItemData == null) yield break;
        
        ItemData data = pendingItemData;
        pendingItemData = null;
        
        if (data.inventoryData != null && Inventory.Instance != null)
        {
            Debug.Log($"임시 데이터에서 인벤토리 아이템 복원 - 아이템 수: {data.inventoryData.inventoryItems.Count}, 핫바 아이템 수: {data.inventoryData.hotbarItems.Count}");
            
            ClearAllInventorySlots();
            yield return null;
            
            foreach (var itemData in data.inventoryData.inventoryItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    RestoreItemToInventorySlot(item, itemData.quantity, itemData.slotIndex, false);
                }
            }
            
            foreach (var itemData in data.inventoryData.hotbarItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    RestoreItemToInventorySlot(item, itemData.quantity, itemData.slotIndex, true);
                }
            }
            
            Inventory.Instance.SelectHotbarSlot(data.inventoryData.currentHotbarSlotIndex);
        }
        
        if (data.storageData != null && Storage.Instance != null)
        {
            Debug.Log($"임시 데이터에서 창고 아이템 복원 - 아이템 수: {data.storageData.storageItems.Count}");
            
            ClearAllStorageSlots();
            yield return null;
            
            foreach (var itemData in data.storageData.storageItems)
            {
                Item item = LoadItemByName(itemData.itemName);
                if (item != null)
                {
                    RestoreItemToStorageSlot(item, itemData.quantity, itemData.slotIndex);
                }
            }
        }
        
        Debug.Log("임시 아이템 데이터 적용 완료!");
    }
    
    public void OnInventoryStorageReady()
    {
        if (pendingItemData != null)
        {
            StartCoroutine(ApplyPendingItemData());
        }
    }
}