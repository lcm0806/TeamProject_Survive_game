using DesignPattern;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerSpawnPoint;

    public bool IsInIntercation = false;
    public WorldItem InteractableItem { get; set; }
    public Structure InteractableStructure { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();
    public PlayerController Player { get; private set; }
    public float InteractDelay { get; set; }
    public bool IsInAirChamber { get; set; } = false;

    private void Awake()
    {
        SingletonInit();
        Init();
    }

    private void Start()
    {
        PlayerInit();
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
    
    private void PlayerInit()
    {
        // 플레이어 임시 생성 코드
        GameObject player = Instantiate(_playerPrefab, _playerSpawnPoint.position, _playerSpawnPoint.rotation);
    }
}
