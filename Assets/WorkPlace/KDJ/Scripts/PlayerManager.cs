using DesignPattern;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public bool IsInIntercation = false;
    public WorldItem InteractableItem { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();
    //테스트용 인벤토리

    public bool IsInAirChamber { get; set; } = false;

    private void Awake()
    {
        SingletonInit();
        Init();
    }

    private void Update()
    {
        TestCode();
    }

    private void Init()
    {
        AirGauge.Value = 100f;
        ElecticGauge.Value = 100f;
    }

    /// <summary>
    /// 테스트용 코드
    /// </summary>
    private void TestCode()
    {
        if (!IsInAirChamber)
            AirGauge.Value -= Time.deltaTime * 1f;
    }
    
}
