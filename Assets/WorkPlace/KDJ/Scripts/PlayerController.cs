using Cinemachine;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 만약 시간이 난다면 기능별로 클래스 분리 고려하기

    #region 직렬화 변수
    [SerializeField] private CharacterController _controller;
    [SerializeField] private CinemachineVirtualCamera _virCam;
    [SerializeField] private Transform _virCamAxis;
    [SerializeField] private Transform _playerHand;
    [SerializeField] private GameObject _mineGunPrefab; // 테스트용 마인건 프리팹

    [Header("Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;
    [SerializeField] private float _speed;
    [SerializeField] private float _baseSpeed = 5f;
    #endregion

    #region 변수
    private Vector3 _verVelocity;
    private Vector3 _moveDir;
    private Vector3 _rayEndPos;
    private Vector3 _fixedDir;
    private Vector3 _groundNormal = Vector3.up; // 땅의 법선 벡터
    private Vector2 _mouseInput;
    private LayerMask _ignoreMask = ~(1 << 3);
    private LayerMask _layerMask = 1 << 6;
    private RaycastHit _rayHit;
    private Collider[] _colls = new Collider[10];
    private Collider _curHitColl;
    private Collider _lastHitColl;
    private Animator _animator;
    private JetPack _jetPack;
    private bool _isRayHit;
    private bool _isMoving => _moveDir != Vector3.zero;
    private bool _canMove => !_isSlipping && !_isUsingJetPack;
    private bool _isGrabbing => PlayerManager.Instance.SelectItem != null;
    private bool _testBool;
    private bool _isMiningPrev = false;
    private bool _isRunning = false;
    private bool _isMining => _testBool;
    private bool _isJumping = false;
    private bool _isUsingJetPack = false;
    private bool _isSlipping => _groundCos < _slopeCos && _controller.isGrounded; // 경사면에서 미끄러지는지 여부
    private bool _isStuck = false; // 경사에 끼인 경우 
    private float _totalMouseY = 0f;
    private float _groundCos;
    private float _slopeCos;
    private float _curY;
    private float _lastY;
    public GameObject _testHandItem;
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        PlayerInput();
        HandlePlayer();
        Animation();
        MineGunSetPos(); // 테스트용 마인건 위치 설정
        Debug.Log("끼였는가? : " + _isStuck);
        Debug.Log("미끄러지는가? :" + _isSlipping);
    }

    private void LateUpdate()
    {
        OnControllerColliderExit(); // 콜라이더에서 벗어났는지 체크
        StuckCheck(); // 플레이어가 끼인 상태인지 확인
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _curHitColl = hit.collider; // 현재 충돌한 콜라이더를 저장
        _groundNormal = hit.normal; // 충돌한 표면의 법선 벡터를 저장
        _groundCos = Vector3.Dot(hit.normal, Vector3.up);
    }

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.92f, 0), 2.5f);

        if (_isRayHit)
        {
            Gizmos.DrawLine(Camera.main.transform.position, _rayHit.point);
            Gizmos.DrawWireSphere(_rayHit.point, 2.5f);
        }
        else
        {
            Gizmos.DrawLine(Camera.main.transform.position, _rayEndPos);
            Gizmos.DrawWireSphere(_rayEndPos, 2.5f);
        }
    }

    private void Init()
    {
        // 테스트용 마우스 숨기기
        _animator = GetComponentInChildren<Animator>();
        _jetPack = GetComponent<JetPack>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _speed = _baseSpeed;
        _slopeCos = Mathf.Cos(_controller.slopeLimit * Mathf.Deg2Rad); // slopeLimit를 라디안으로 변환하여 코사인 값 계산
    }

    private void OnControllerColliderExit()
    {
        // 콜라이더에 닿았었는데 이제는 닿지 않는 경우
        if (_lastHitColl != null && _curHitColl == null)
        {
            // 움직임을 고정 방향으로 설정
            _fixedDir = transform.TransformDirection(_moveDir) * _speed * 0.5f;
        }

        _lastHitColl = _curHitColl; // 현재 콜라이더를 마지막 콜라이더로 저장
        _curHitColl = null; // 현재 콜라이더를 null로 초기화
    }

    #region 상호작용 : 레이캐스트의 위치에서 감지
    private void FindCloseInteractableItemAtRay()
    {
        // overlapsphere를 플레이어 위치가 아닌 레이의 끝지점에서 생성
        // 스크린 중앙 기준 가장 가까이 있는 오브젝트를 _interactableItem로 설정
        // 레이캐스트로 중앙을 감지하고 감지된 hit 기준 거리 계산
        Collider closestColl = null;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        _isRayHit = Physics.Raycast(ray, out _rayHit, 6f);
        int collsCount = 0;
        Gizmos.color = Color.green;

        if (_isRayHit)
        {
            // 레이캐스트가 성공하면 hit.point를 기준으로 overlapsphere를 생성
            collsCount = Physics.OverlapSphereNonAlloc(_rayHit.point, 2.5f, _colls, _layerMask);
        }
        else
        {
            // 실패시 카메라의 위치에서 레이 방향으로 6f 떨어진 지점에서 overlapsphere를 생성
            _rayEndPos = _virCamAxis.position + Camera.main.transform.forward * 2f;
            collsCount = Physics.OverlapSphereNonAlloc(_rayEndPos, 2.5f, _colls, _layerMask);
        }

        if (collsCount > 0)
        {
            for (int i = 0; i < collsCount; i++)
            {
                if (_isRayHit)
                {
                    // 레이캐스트가 성공한 경우 hit.point에서 오브젝트의 거리 측정
                    float distance = Vector3.Distance(_rayHit.point, _colls[i].transform.position);
                    // closestColl이 null이거나 현재 오브젝트가 closestColl보다 가까운 경우 현재 인덱스의 콜라이더를 closestColl로 설정
                    if (closestColl == null || distance < Vector3.Distance(_rayHit.point, closestColl.transform.position))
                    {
                        closestColl = _colls[i];
                    }
                }
                else
                {
                    // 레이캐스트가 실패한 경우 rayEndPos에서 오브젝트의 거리 측정
                    float distance = Vector3.Distance(_rayEndPos, _colls[i].transform.position);
                    // closestColl이 null이거나 현재 오브젝트가 closestColl보다 가까운 경우 현재 인덱스의 콜라이더를 closestColl로 설정
                    if (closestColl == null || distance < Vector3.Distance(_rayEndPos, closestColl.transform.position))
                    {
                        closestColl = _colls[i];
                    }
                }
            }
            // 끝나면 closestColl의 내용을 _interactableItem에 할당
            if (closestColl != null && closestColl.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                if (interactable as WorldItem)
                {
                    PlayerManager.Instance.InteractableItem = interactable as WorldItem;
                    PlayerManager.Instance.IsInIntercation = true;
                }
                else if (interactable as Structure)
                {
                    PlayerManager.Instance.InteractableStructure = interactable as Structure;
                    PlayerManager.Instance.IsInIntercation = true;
                }
                // 아래는 테스트 코드
                else if (interactable as TestWorldItem)
                {
                    PlayerManager.Instance.InteractableTestItem = interactable as TestWorldItem;
                    PlayerManager.Instance.IsInIntercation = true;
                }
            }
        }
        else
        {
            // 주변에 인터렉션 가능한 오브젝트가 없으면 상호작용을 null로 설정
            PlayerManager.Instance.InteractableStructure = null;
            PlayerManager.Instance.InteractableItem = null;
            PlayerManager.Instance.IsInIntercation = false;
        }
    }
    #endregion

    private void HandlePlayer()
    {
        AimControl();
        PlayerEffect();
        Move();
        Jump();
        Run();
        CameraLimit();
        FindCloseInteractableItemAtRay();
    }
    private void PlayerInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _mouseInput = new Vector2(mouseX, mouseY);


        #region E 키 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 땅에 떨어진 아이템은 E누르면 즉시 상호작용
            if (PlayerManager.Instance.InteractableItem != null)
            {
                Debug.Log($"[PlayerManager] E키 눌림. 상호작용할 아이템: {PlayerManager.Instance.InteractableItem.name}");
                // PlayerManager.Instance.InteractableItem.Interact();
                PlayerManager.Instance.InteractableItem.Interact(); // 테스트 코드
            }
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (PlayerManager.Instance.InteractableStructure != null)
            {
                // 구조물의 경우 1초간 눌러야만 상호작용
                PlayerManager.Instance.InteractDelay += 1 * Time.deltaTime;

                if (PlayerManager.Instance.InteractDelay >= 1f)
                {
                    PlayerManager.Instance.InteractableStructure.Interact();
                    PlayerManager.Instance.InteractDelay = 0f; // 상호작용 후 딜레이 초기화
                }
            }
            else
            {
                PlayerManager.Instance.InteractDelay = 0f; // 중간에 다른곳을 바라보아도 초기화
            }
        }
        else
        {
            PlayerManager.Instance.InteractDelay = 0f; // E키를 떼면 초기화
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Q)) // 'Q' 키를 눌렀을 때
        {
            SampleUIManager.Instance.ToggleInventoryUI(); // SampleUIManager의 인벤토리 토글 메서드 호출
        }

        #region 핫바 선택

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerManager.Instance.SelectItem = null;
            Destroy(_testHandItem);
            // 인벤토리 핫바 1번 선택
            Inventory.Instance.SelectHotbarSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerManager.Instance.SelectItem = null;
            Destroy(_testHandItem);
            // 인벤토리 핫바 2번 선택
            Inventory.Instance.SelectHotbarSlot(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerManager.Instance.SelectItem = null;
            Destroy(_testHandItem);
            // 인벤토리 핫바 3번 선택
            Inventory.Instance.SelectHotbarSlot(2);
        }

        Item curItem = Inventory.Instance.GetCurrentHotbarItem();

        if (curItem == null)
        {
            PlayerManager.Instance.SelectItem = null; // 현재 핫바에 아이템이 없으면 선택 아이템을 null로 설정
            if (_testHandItem != null)
            {
                // 테스트용 마인건이 있다면 비활성화
                // _testHandItem.SetActive(false);
                Destroy(_testHandItem); // 오브젝트 제거
            }
            // _testHandItem = null; // 테스트용 마인건도 null로 설정
        }
        else
        {
            PlayerManager.Instance.SelectItem = curItem; // 현재 핫바에 아이템이 있으면 선택 아이템으로 설정

            if (_testHandItem == null)
            {
                // 소비아이템의 프리팹을 생성하는 로직 생성
                _testHandItem = Instantiate(curItem.HandleItem, _playerHand.position, _playerHand.rotation);
            }
        }
        #endregion

        #region 아이템 사용
        if (Input.GetMouseButtonDown(0) && PlayerManager.Instance.SelectItem as MaterialItem && !SampleUIManager.Instance.inventoryPanel.activeSelf)
        {
            // 손에 자원 아이템이 들려 있는 경우
            _animator.SetTrigger("Swing");
        }
        else if (Input.GetMouseButton(0) && PlayerManager.Instance.SelectItem as ToolItem && !SampleUIManager.Instance.inventoryPanel.activeSelf)
        {
            _testBool = true; // 마이닝 애니메이션 실행을 위한 bool 값 설정

            // 아이템 사용은 중간에 마우스를 때면 멈춰야 하기에 코루틴이 아닌 그냥 구현
            PlayerManager.Instance.ItemDelay += Time.deltaTime;
            if (PlayerManager.Instance.ItemDelay >= 0.1f)
            {
                Debug.Log("아이템 사용!");
                PlayerManager.Instance.SelectItem.Use(this.gameObject);
                PlayerManager.Instance.ItemDelay = 0f; // 아이템 사용 후 딜레이 초기화
            }
        }
        else if (Input.GetMouseButton(0) && PlayerManager.Instance.SelectItem as ConsumableItem && !SampleUIManager.Instance.inventoryPanel.activeSelf)
        {
            PlayerManager.Instance.ItemDelay += Time.deltaTime;
            if (PlayerManager.Instance.ItemDelay >= 1f)
            {
                Debug.Log("아이템 사용!");
                PlayerManager.Instance.SelectItem.Use(this.gameObject);
                PlayerManager.Instance.ItemDelay = 0f; // 아이템 사용 후 딜레이 초기화
            }
        }
        else
        {
            _testBool = false;
            PlayerManager.Instance.ItemDelay = 0f; // 마우스를 떼면 아이템 사용 딜레이 초기화
        }
        #endregion

        // 제트팩은 공중에서만 사용
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_controller.isGrounded && PlayerManager.Instance.IsUpgraded[0])
        {
            _isUsingJetPack = true;
            _moveDir = Vector3.zero; // 제트팩 사용시 이동 방향 초기화
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && _isUsingJetPack || _controller.isGrounded)
        {
            _isUsingJetPack = false;
        }

        if (!_canMove) return; // 움직일 수 없다면 이동은 생략

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        _moveDir = new Vector3(x, 0, y).normalized;
    }

    #region 플레이어 이동
    private void Move()
    {
        _curY = transform.position.y; // 현재 y축 위치 저장

        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함
        Vector3 move = transform.TransformDirection(_moveDir) * _speed;

        // 화성이 배경이니 중력은 5.5
        _verVelocity.y -= 5.5f * Time.deltaTime; // 중력 적용

        // 제트팩 사용
        if (_isUsingJetPack)
        {
            _verVelocity.y += 1.5f * Time.deltaTime; // 제트팩 사용시 중력 약화

            _fixedDir = _jetPack.UseUpgrade(Camera.main.transform.forward);

            if (_fixedDir != Vector3.zero)
            {
                _verVelocity.y += _fixedDir.y * Time.deltaTime; // 제트팩의 힘을 적용
            }
        }

        if (_isSlipping && _controller.isGrounded)
        {
            // 경사면인 경우에는 중력을 적용하고 땅의 법선벡터방향으로 밀어서 미끌어지게 만듬
            _fixedDir = Vector3.zero; // 고정 방향 초기화
            _moveDir = Vector3.zero; // 이동 방향 초기화
            move = _groundNormal * 2f; // 땅의 법선 벡터 방향으로 밀어서 미끌어지게 함
            _controller.Move((move + _verVelocity) * 2f * Time.deltaTime);
            return;
        }

        if (_controller.isGrounded && !_isJumping)
        {
            _verVelocity.y = -2f;
        }

        if (!_controller.isGrounded)
        {
            // 기존 운동량 보존. 플레이어의 입력은 20%만 적용
            Vector3 airDir = _fixedDir + (transform.TransformDirection(_moveDir) * _speed * 0.2f);
            _controller.Move((airDir + _verVelocity) * Time.deltaTime);
            return;
        }

        _controller.Move((move + _verVelocity) * Time.deltaTime);

    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_controller.isGrounded && !_isSlipping || _isStuck)
            {
                _isJumping = true; // 점프 상태로 변경
                // 점프력
                _verVelocity.y = 7f;
                // 점프시 이동 방향을 고정
                _fixedDir = transform.TransformDirection(_moveDir) * _speed;
            }
        }
        else
        {
            _isJumping = false; // 점프 상태 해제
        }
    }

    private void Run()
    {
        // 달리기 기능는 테스트상 편하기 위해 넣은 것. 정식 빌드에선 제거
        if (Input.GetKeyDown(KeyCode.LeftShift) && _controller.isGrounded)
        {
            _speed *= 2;
            _isRunning = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && _isRunning)
        {
            _speed /= 2;
            _isRunning = false;
        }
    }
    #endregion

    #region 플레이어 시점 관련
    private void AimControl()
    {
        Vector2 mouseInput = _mouseInput * _mouseSensitivity;
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
        if (_testHandItem != null)
        {
            // 플레이어의 손 위치에 마인건을 위치시킴
            _testHandItem.transform.position = _playerHand.position;
            _testHandItem.transform.rotation = _playerHand.rotation;
        }
    }

    private void StuckCheck()
    {
        if (_isSlipping && _controller.isGrounded)
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
            PlayerSlow(50f);
        }
        else
        {
            // 마이닝 종료시 슬로우 제거
            RemoveSlow(50f);
        }

        _isMiningPrev = _isMining;
    }

    /// <summary>
    /// 슬로우 강도를 퍼센테이지로 입력 받아 플레이어 감속
    /// </summary>
    /// <param name="percentage">0~100 사이의 값으로 입력, 0은 감속 없음, 100은 정지</param>
    public void PlayerSlow(float percentage)
    {
        _speed = _speed * (1f - percentage / 100f);
    }

    /// <summary>
    /// 슬로우의 역순으로 계산하기에 PlayerSlow에서 입력한 수치를 그대로 입력해야함
    /// </summary>
    /// <param name="percentage"></param>
    public void RemoveSlow(float percentage)
    {
        // 슬로우의 역순
        _speed = _speed / (1f - percentage / 100f);
    }
    #endregion

    #region 미사용 코드


    #endregion

    #region 애니메이션
    private void Animation()
    {
        MoveAnim();
        GrabAnim();
        // SwingAnim();
        MiningAnim();
    }

    private void MoveAnim()
    {
        _animator.SetBool("IsWalking", _isMoving);
    }

    private void GrabAnim()
    {
        _animator.SetBool("IsGrabbing", _isGrabbing);
    }

    private void SwingAnim()
    {
        // 나중엔 _isGrabbing도 조건에 추가되도록 변경
        if (Input.GetMouseButtonDown(0) && _isGrabbing)
        {
            _animator.SetTrigger("Swing");
        }
    }

    private void MiningAnim()
    {
        _animator.SetBool("IsMining", _testBool);
    }
    #endregion
}
