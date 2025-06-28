using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    
    // static = 게임 어디서든 접근 가능한 변수
    private static StatusSystem _instance;
    
    // 다른 스크립트에서 GameManager.Instance 로 접근할 수 있게 해주는 속성
    public static StatusSystem Instance
    {
        get
        {
            // 만약 instance가 없다면 찾아보고, 그래도 없으면 새로 만듭니다
            if (_instance == null)
            {
                _instance = FindObjectOfType<StatusSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("StatusSystem");
                    _instance = go.AddComponent<StatusSystem>();
                }
            }
            return _instance;
        }
    }
    
    private int _currentDay = 1; // 현재 날짜 (1일부터 시작)
    private double _oxygenRemaining = 100f; // 남은 산소량 (100에서 시작)
    private double _electricalEnergy = 100f; // 전력 (100에서 시작)
    private double _shelterDurability = 100f; // 쉘터 내구도 (100에서 시작)
    private bool _isToDay= false; // 오늘 탐색했는지 여부
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // 부모가 있다면 루트로 이동 (DontDestroyOnLoad를 위해)
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);
    }


    /// <summary>
    /// 현재 산소 얻기
    /// </summary>
    /// <returns></returns>
    public double GetOxygen()
    {
        return _oxygenRemaining;
    }

    /// <summary>
    /// 산소 플러스
    /// </summary>
    /// <param name="value"></param>
    public void SetPlusOxygen(double value)
    {
        _oxygenRemaining += value;
    }
    
    /// <summary>
    /// 산소 마이너스
    /// </summary>
    /// <param name="value"></param>
    public void SetMinusOxygen(double value)
    {
        _oxygenRemaining -= value;
    }
    
    
    /// <summary>
    /// 현재 전력 얻기
    /// </summary>
    /// <returns></returns>
    public double GetEnergy()
    {
        return _electricalEnergy;
    }

    /// <summary>
    ///  전력 올리기
    /// </summary>
    /// <param name="value"></param>
    public void SetPlusEnergy(double value)
    {
        _electricalEnergy += value;
    }
    
    /// <summary>
    /// 전력 깍기
    /// </summary>
    /// <param name="value"></param>
    public void SetMinusEnergy(double value)
    {
        _electricalEnergy -= value;
    }
    
    
    
    
    
    /// <summary>
    /// 현재 내구도 얻기
    /// </summary>
    /// <returns></returns>
    public double GetDurability()
    {
        return _shelterDurability;
    }

    /// <summary>
    /// 내구도 올리기
    /// </summary>
    /// <param name="value"></param>
    public void SetPlusDurability(double value)
    {
        _shelterDurability += value;
    }
    
    /// <summary>
    /// 내구도 깍기
    /// </summary>
    /// <param name="value"></param>
    public void SetMinusDurability(double value)
    {
        _shelterDurability -= value;
    }
    
    
    
    
    /// <summary>
    /// 현재 날
    /// </summary>
    /// <returns></returns>
    public int GetCurrentDay()
    {
        return _currentDay;
    }
    
    /// <summary>
    /// 다음날 이동
    /// </summary>
    public void NextCurrentDay()
    {
        _currentDay = GetCurrentDay() + 1;
    }
    
    
    
    
    
    /// <summary>
    /// 탐색을 했는지 여부
    /// </summary>
    /// <param name="value"></param>
    public bool GetIsToDay()
    {
        return _isToDay;
    }

    /// <summary>
    /// 탐색을 했는지 여부
    /// </summary>
    /// <param name="value"></param>
    public void SetIsToDay(bool value)
    {
        _isToDay = value;
    }
    
    
    
}
