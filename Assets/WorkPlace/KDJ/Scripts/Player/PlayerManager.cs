using DesignPattern;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private float _baseSpeed = 5f;

    [field: SerializeField] public GameObject SunGlasses;

    private Akimbo _akimboCheck;
    
    public float Speed;
    /// <summary>
    /// 플레이어 강화 상태를 표시하는 Bool 배열. 크기는 3. 0 = 제트팩, 1,2는 차후 추가 예정
    /// </summary>
    public bool[] IsUpgraded { get; set; } = new bool[3];
    public bool IsInIntercation = false;
    public bool IsAkimbo { get; set; } = false;
    public WorldItem InteractableItem { get; set; }
    public TestWorldItem InteractableTestItem { get; set; }
    public Structure InteractableStructure { get; set; }
    public GameObject InHandItem { get; set; }
    public GameObject InHandItem2 { get; set; } 
    public GameObject InHeadItem { get; set; } // 헬멧, 선글라스 등 머리에 착용하는 아이템
    public Item SelectItem { get; set; }
    public ObseravableProperty<float> AirGauge = new();
    public ObseravableProperty<float> ElecticGauge = new();
    public PlayerController Player { get; private set; }
    public RaycastHit HitInfo { get; set; }
    public float InteractDelay { get; set; }
    public float ItemDelay { get; set; }


    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Start()
    {
        PlayerInit();
    }

    private void Update()
    {
        if (SceneSystem.Instance?.GetCurrentSceneName() == SceneSystem.Instance?.GetFarmingSceneName() && Player == null)
        {
            PlayerInit();
        }

        AkimboCheck(); // 아킴보 상태 확인
    }

    private void Init()
    {
        AirGauge.Value = 100f;
        ElecticGauge.Value = 100f;
        Speed = _baseSpeed;
    }

    private void PlayerInit()
    {


        GameObject player = Instantiate(_playerPrefab, new Vector3(255.087f, 10.225f, -123.6639f), Quaternion.identity);

        Player = player.GetComponent<PlayerController>();

    }

    private void AkimboCheck()
    {
        if (_akimboCheck != null) return;


        if (SelectItem != null)
        {
            IsAkimbo = SelectItem.HandleItem.TryGetComponent<Akimbo>(out _akimboCheck);
        }
    }

    public void AkimboReset()
    {
        // 아킴보 상태 초기화
        IsAkimbo = false;
        _akimboCheck = null;
    }
}
