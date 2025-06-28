using DesignPattern;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerSpawnPoint;

    public bool IsInIntercation = false;
    public WorldItem InteractableItem { get; set; }
    public TestWorldItem InteractableTestItem { get; set; }
    public Structure InteractableStructure { get; set; }
    public Item SelectItem { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();
    public PlayerController Player { get; private set; }
    public float InteractDelay { get; set; }
    public float ItemDelay { get; set; }

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
    }

    private void Init()
    {
        AirGauge.Value = 100f;
        ElecticGauge.Value = 100f;
    }
    
    private void PlayerInit()
    {
        // 플레이어 임시 생성 코드
        GameObject player = Instantiate(_playerPrefab, _playerSpawnPoint.position, _playerSpawnPoint.rotation);
        Player = player.GetComponent<PlayerController>();
        Debug.Log("플레이어 생성 완료");
    }
}
