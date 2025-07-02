using UnityEngine;
using DesignPattern;

public class StatusSystem : Singleton<StatusSystem>
{
    
    private int _currentDay = 1; // 현재 날짜 (1일부터 시작)
    private double _oxygenRemaining = 100f; // 남은 산소량 (100에서 시작)
    private double _electricalEnergy = 100f; // 전력 (100에서 시작)
    private double _shelterDurability = 100f; // 쉘터 내구도 (100에서 시작)
    private bool _isToDay; // 오늘 탐색했는지 여부

    //획득효율감소이벤트관련 배율처리변수추가 25.7.2. 이학권
    private double _oxygenGainMultiplier = 1.0;
    private double _energyGainMultiplier = 1.0;

    void Awake()
    {
        SingletonInit();
    }

    public void SetCurrentDay(int day)
    {
        _currentDay = day;
    }
    
    /// <summary>
    /// 현재 산소 설정
    /// </summary>
    /// <returns></returns>
    public void SetOxygen(double value)
    {
        _oxygenRemaining = value;
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
        _oxygenRemaining += value * _oxygenGainMultiplier;
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
    /// 현재 전력 얻기
    /// </summary>
    /// <returns></returns>
    public void SetEnergy(double value)
    {
        _electricalEnergy = value;
    }

    /// <summary>
    ///  전력 올리기
    /// </summary>
    /// <param name="value"></param>
    public void SetPlusEnergy(double value)
    {
        _electricalEnergy += value * _energyGainMultiplier;
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
    /// 현재 내구도 얻기
    /// </summary>
    /// <returns></returns>
    public void SetDurability(double value)
    {
        _shelterDurability = value;
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
    

    //산소와 전력 획득효율감소 함수 25.7.2. 이학권
    public void SetMinusOxygenGainMultiplier(double value)
    {
        _oxygenGainMultiplier = -value;
    }

    public void SetPlusOxygenGainMultiplier(double value)
    {
        _oxygenGainMultiplier = +value;
    }

    public void SetMinusEnergyGainMultiplier(double value)
    {
        _energyGainMultiplier = -value;
    }
    public void SetPlusEnergyGainMultiplier(double value)
    {
        _energyGainMultiplier = +value;
    }
}
