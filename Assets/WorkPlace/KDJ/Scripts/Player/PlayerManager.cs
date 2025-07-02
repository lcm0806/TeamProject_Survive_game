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
    public bool IsAkimbo { get; set; } = false; // 아킴보 상태 여부
    public WorldItem InteractableItem { get; set; }
    public TestWorldItem InteractableTestItem { get; set; }
    public Structure InteractableStructure { get; set; }
    public GameObject InHandItem { get; set; }
    public GameObject InHandItem2 { get; set; } // 아킴보 상태일 때 두번째 아이템
    public GameObject InHeadItem { get; set; } // 헬멧, 선글라스 등 머리에 착용하는 아이템
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
        if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName() && Player == null)
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
        // 플레이어 임시 생성 코드
        GameObject player = Instantiate(_playerPrefab, new Vector3(255.86f, 10.24f, -123.64f), Quaternion.identity);
        Player = player.GetComponent<PlayerController>();
        Debug.Log("플레이어 생성 완료");
    }

    private void AkimboCheck()
    {
        if (_akimboCheck != null) return; // 이미 아킴보를 받아왔다면 탈출


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
