using DesignPattern;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public bool IsInIntercation = false;
    public TestItem InteractableItem { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();
    //테스트용 인벤토리
    public TestItem[] Inventory = new TestItem[5];

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

    public void AddItemToInventory(TestItem item)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == null)
            {
                Inventory[i] = item;
                break;
            }
        }
    }
}
