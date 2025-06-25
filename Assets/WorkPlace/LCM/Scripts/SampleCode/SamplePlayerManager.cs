using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class SamplePlayerManager : Singleton<SamplePlayerManager>
{
    //[SerializeField] private GameObject _playerPrefab;
    //[SerializeField] private Transform _playerSpawnPoint;

    public bool IsInIntercation = false;
    public WorldItem InteractableItem { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();

    [field:SerializeField]
    public PlayerController Player { get; private set; }
    //테스트용 인벤토리

    public bool IsInAirChamber { get; set; } = false;

    private void Awake()
    {
        SingletonInit();
        Init();

        
    }

    private void Start()
    {
        //PlayerInit();
    }

    private void Update()
    {
        //TestCode();
    }

    private void Init()
    {
        AirGauge.Value = 100f;
        ElecticGauge.Value = 100f;
    }

    /// <summary>
    /// 테스트용 코드
    /// </summary>
    //private void TestCode()
    //{
    //    if (!IsInAirChamber)
    //        AirGauge.Value -= Time.deltaTime * 1f;
    //}

    //private void PlayerInit()
    //{
    //    // 플레이어 임시 생성 코드
    //    GameObject player = Instantiate(_playerPrefab, _playerSpawnPoint.position, _playerSpawnPoint.rotation);
    //}
}
