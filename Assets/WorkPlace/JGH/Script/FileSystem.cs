using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;
using System.Reflection; // 리플렉션을 위해 추가


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

public class FileSystem : Singleton<FileSystem>
{
    private string settingPath;
    private string gameDataPath;
    
    void Awake()
    {
        SingletonInit();
        settingPath = Path.Combine(Application.persistentDataPath, "setting.json");
        gameDataPath = Path.Combine(Application.persistentDataPath, "gamedata.json");
    }
    // 저장
    public void SaveSetting(SettingData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(settingPath, json);
            Debug.Log("설정 저장 완료: " + settingPath);
        }
        catch (Exception e)
        {
            Debug.LogError("설정 저장 실패: " + e.Message);
        }
    }

    // 불러오기
    public SettingData LoadSetting()
    {
        try
        {
            if (File.Exists(settingPath))
            {
                string json = File.ReadAllText(settingPath);
                SettingData data = JsonUtility.FromJson<SettingData>(json);
                Debug.Log("설정 불러오기 완료");
                return data;
            }
            else
            {
                Debug.Log("설정 파일 없음, 기본값 반환");
                return GetDefaultSetting();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("설정 불러오기 실패: " + e.Message);
            return GetDefaultSetting();
        }
    }

    // 기본값
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
    
    
    // 게임 데이터 저장
    public void SaveGameData(GameData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(gameDataPath, json);
            Debug.Log("게임 데이터 저장 완료: " + gameDataPath);
        }
        catch (Exception e)
        {
            Debug.LogError("게임 데이터 저장 실패: " + e.Message);
        }
    }

    public void ApplyGameData(GameData data)
    {
        StatusSystem.Instance.SetCurrentDay(data.currentDay);
        StatusSystem.Instance.SetOxygen(data.oxygenRemaining);
        StatusSystem.Instance.SetEnergy(data.electricalEnergy);
        StatusSystem.Instance.SetDurability(data.shelterDurability);
        StatusSystem.Instance.SetIsToDay(data.isToDay);
        
        MenuSystem.Instance.MainMenu.SetActive(false);
    }
    
    // 게임 데이터 불러오기
    public GameData LoadGameData()
    {
        try
        {
            if (File.Exists(gameDataPath))
            {
                string json = File.ReadAllText(gameDataPath);
                GameData data = JsonUtility.FromJson<GameData>(json);
                Debug.Log("게임 데이터 불러오기 완료");
                return data;
            }
            else
            {
                Debug.Log("게임 데이터 없음, 기본값 반환");
                return GetDefaultGameData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("게임 데이터 불러오기 실패: " + e.Message);
            return GetDefaultGameData();
        }
    }
    
    

    // 게임 데이터 기본값
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
}