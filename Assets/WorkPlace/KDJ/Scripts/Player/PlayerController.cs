using Cinemachine;
using DesignPattern;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 만약 시간이 난다면 기능별로 클래스 분리 고려하기

    #region 직렬화 변수
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Transform _virCamAxis;
    [SerializeField] private CinemachineVirtualCamera _virCam;
    [SerializeField] private Transform _playerHand;
    [SerializeField] private GameObject _mineGunPrefab; // 테스트용 마인건 프리팹

    [Header("Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;
    #endregion

    #region 변수
    
    public Vector3 FixedDir { get; set; } // 고정 방향 벡터, 플레이어가 움직일 때 사용
    public CharacterController Controller => _controller;
    public PlayerInteraction PlayerInteraction => _playerInteraction;
    public Transform VirCamAxis => _virCamAxis;
    public Transform PlayerHand => _playerHand;
    public bool IsUsingJetPack { get; set; } = false;
    public bool IsSlipping => _playerInteraction.GroundCos < _playerInteraction.SlopeCos && Controller.isGrounded; // 경사면에서 미끄러지는지 여부

    private Vector3 _verVelocity;
    private LayerMask _ignoreMask = ~(1 << 3);
    private PlayerInteraction _playerInteraction;
    private PlayerAnimation _playerAnimation;
    private JetPack _jetPack;
    private bool _isMiningPrev = false;
    private bool _isRunning = false;
    private bool _isMining => InputManager.Instance.IsUsingTool;
    private bool _isJumping = false;
    private bool _isStuck = false; // 경사에 끼인 경우 
    private float _totalMouseY = 0f;
    private float _curY;
    private float _lastY;
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        HandlePlayer();
        MineGunSetPos(); // 테스트용 마인건 위치 설정
        DebugLog(); // 디버그 로그 출력
    }

    private void LateUpdate()
    {
        StuckCheck(); // 플레이어가 끼인 상태인지 확인
    }

    private void Init()
    {
        _jetPack = GetComponent<JetPack>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        // 테스트용 마우스 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DebugLog()
    {
        // Debug.Log("끼였는가? : " + _isStuck);
        // Debug.Log("미끄러지는가? :" + IsSlipping);
        // Debug.Log("땅에 붙어있는가?: " + Controller.isGrounded);
    }

    private void HandlePlayer()
    {
        AimControl();
        PlayerEffect();
        Move(ref PlayerManager.Instance.Speed);
        Jump(ref PlayerManager.Instance.Speed);
        Run(ref PlayerManager.Instance.Speed);
        CameraLimit();
        _playerInteraction.FindCloseInteractableItemAtRay();
    }

    #region 플레이어 이동
    private void Move(ref float speed)
    {
        _curY = transform.position.y; // 현재 y축 위치 저장

        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함
        Vector3 move = transform.TransformDirection(InputManager.Instance.MoveDir) * speed;
        

        // 화성이 배경이니 중력은 5.5
        _verVelocity.y -= 5.5f * Time.deltaTime; // 중력 적용

        // 제트팩 사용
        if (IsUsingJetPack)
        {
            _verVelocity.y += 3.5f * Time.deltaTime; // 제트팩 사용시 중력 약화

            // 제트팩이 너무 쳐지는걸 막기 위해 살짝 위쪽을 향하도록 vector3.up을 더함
            FixedDir = _jetPack.UseUpgrade(Camera.main.transform.forward) + Vector3.up;

            if (FixedDir != Vector3.zero)
            {
                _verVelocity.y += FixedDir.y * Time.deltaTime; // 제트팩의 힘을 적용
            }
        }

        if (IsSlipping && Controller.isGrounded)
        {
            // 경사면인 경우에는 중력을 적용하고 땅의 법선벡터방향으로 밀어서 미끌어지게 만듬
            FixedDir = Vector3.zero; // 고정 방향 초기화
            InputManager.Instance.MoveDir = Vector3.zero; // 이동 방향 초기화
            move = _playerInteraction.GroundNormal * 2f; // 땅의 법선 벡터 방향으로 밀어서 미끌어지게 함
            Controller.Move((move + _verVelocity) * 2f * Time.deltaTime);
            return;
        }

        if (Controller.isGrounded && !_isJumping)
        {
            _verVelocity.y = -2f;
        }

        if (!Controller.isGrounded)
        {
            // 기존 운동량 보존. 플레이어의 입력은 20%만 적용
            Vector3 airDir = FixedDir + (transform.TransformDirection(InputManager.Instance.MoveDir) * speed * 0.2f);
            Controller.Move((airDir + _verVelocity) * Time.deltaTime);
            return;
        }

        Controller.Move((move + _verVelocity) * Time.deltaTime);

    }

    private void Jump(ref float speed)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Controller.isGrounded && !IsSlipping || _isStuck)
            {
                _isJumping = true; // 점프 상태로 변경
                // 점프력
                _verVelocity.y = 7f;
                // 점프시 이동 방향을 고정
                FixedDir = transform.TransformDirection(InputManager.Instance.MoveDir) * speed;
            }
        }
        else
        {
            _isJumping = false; // 점프 상태 해제
        }
    }

    private void Run(ref float speed)
    {
        // 달리기 기능는 테스트상 편하기 위해 넣은 것. 정식 빌드에선 제거
        if (Input.GetKeyDown(KeyCode.LeftShift) && Controller.isGrounded)
        {
            speed *= 2;
            _isRunning = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && _isRunning)
        {
            speed /= 2;
            _isRunning = false;
        }
    }
    #endregion

    #region 플레이어 시점 관련
    private void AimControl()
    {
        Vector2 mouseInput = InputManager.Instance.MouseInput * _mouseSensitivity;
        _totalMouseY = Mathf.Clamp(_totalMouseY - mouseInput.y, -75, 75);

        float clampedMouseY = _virCam.transform.localEulerAngles.x - mouseInput.y;
        if (clampedMouseY > 180)
        {
            clampedMouseY -= 360;
        }

        _virCamAxis.transform.localRotation = Quaternion.Euler(_totalMouseY, 0, 0);

        transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void CameraLimit()
    {
        // 카메라가 벽에 부딪히는 경우 벽 위치만큼 앞으로 이동
        // 축에서 카메라로 레이를 발사하여 벽에 부딪히는 경우, 부딛히는 위치를 계산하여 카메라를 이동시킴
        RaycastHit hit;
        if (Physics.Raycast(_virCamAxis.position, -_virCamAxis.forward, out hit, 4.5f, _ignoreMask))
        {
            Vector3 targetPos = hit.point + _virCam.transform.forward * 0.5f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, targetPos, 0.5f);
        }
        else
        {
            // 벽에 부딪히지 않는 경우 위치 리셋
            Vector3 resetPos = _virCamAxis.position - _virCamAxis.forward * 4f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, resetPos, 0.5f);
        }
    }
    #endregion

    #region 잡다한 코드 코드
    private void MineGunSetPos()
    {
        if (PlayerManager.Instance.InHandItem != null)
        {
            // 플레이어의 손 위치에 마인건을 위치시킴
            PlayerManager.Instance.InHandItem.transform.position = _playerHand.position;
            PlayerManager.Instance.InHandItem.transform.rotation = _playerHand.rotation;
        }
    }

    private void StuckCheck()
    {
        if (IsSlipping && Controller.isGrounded)
        {
            // 경사면에서 미끄러지는 경우, 플레이어가 끼인 상태인지 확인
            // 현재 y축과 마지막 y축을 비교하여 같다면 끼인 상태
            if (Mathf.Abs(_curY - _lastY) < 0.00001f)
            {
                _isStuck = true; // 끼인 상태로 설정
            }
            else
            {
                _isStuck = false; // 끼이지 않은 상태로 설정
            }
        }
        else
        {
            _isStuck = false; // 경사면이 아니거나 점프 중이면 끼이지 않은 상태로 설정
        }

        _lastY = _curY; // 현재 y축을 마지막 y축으로 저장
        _curY = 0f; // 현재 y축 초기화
    }
    #endregion

    #region 아이템 사용에 다른 플레이어 스텟 변화
    private void PlayerEffect()
    {
        MiningSlow();
    }

    private void MiningSlow()
    {
        if (_isMiningPrev.Equals(_isMining)) return;

        if (_isMining)
        {
            // 마이닝 중일 때 슬로우 적용
            PlayerSlow(50f, ref PlayerManager.Instance.Speed);
        }
        else
        {
            // 마이닝 종료시 슬로우 제거
            RemoveSlow(50f, ref PlayerManager.Instance.Speed);
        }

        _isMiningPrev = _isMining;
    }

    /// <summary>
    /// 슬로우 강도를 퍼센테이지로 입력 받아 플레이어 감속
    /// </summary>
    /// <param name="percentage">0~100 사이의 값으로 입력, 0은 감속 없음, 100은 정지</param>
    /// /// <param name="speed">변화시킬 스피드</param>
    public void PlayerSlow(float percentage, ref float speed)
    {
        speed = speed * (1f - percentage / 100f);
    }

    /// <summary>
    /// 슬로우의 역순으로 계산하기에 PlayerSlow에서 입력한 수치를 그대로 입력해야함
    /// </summary>
    /// <param name="percentage"></param>
    /// <param name="speed">변화시킬 스피드</param>
    public void RemoveSlow(float percentage, ref float speed)
    {
        // 슬로우의 역순
        speed = speed / (1f - percentage / 100f);
    }
    #endregion

    #region 미사용 코드


    #endregion

}
