using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 직렬화 변수
    [SerializeField] private CharacterController _controller;
    [SerializeField] private CinemachineVirtualCamera _virCam;
    [SerializeField] private Transform _virCamAxis;

    [Header("Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;
    [SerializeField] private float _speed;
    [SerializeField] private float _baseSpeed = 5f;
    #endregion

    #region 변수
    private Vector3 _verVelocity;
    private Vector3 _moveDir;
    private Vector2 _mouseInput;
    private float _totalMouseY = 0f;
    private LayerMask _ignoreMask = ~(1 << 3);
    private LayerMask _layerMask = 1 << 6;
    private Collider[] _colls = new Collider[10];
    private WorldItem _worldItem;
    private Animator _animator;
    private Vector3 _rayEndPos;
    private bool _isRayHit;
    private RaycastHit _rayHit;
    private bool _isMoving => _moveDir != Vector3.zero;
    // private bool _isGrabbing => _selectItem != null;
    // 아래는 테스트 코드
    private bool _isGrabbing = false;
    private bool _testBool;
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
    }

    private void Init()
    {
        // 테스트용 마우스 숨기기
        _animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _speed = _baseSpeed;
    }

    #region 상호작용 : 플레이어에 가장 가까운 물체
    private void FindCloseInteractableItemFromPlayer()
    {
        // 트리거를 사용하지 않고 overlapsphere를 사용하여 주변의 인터렉션 가능한 오브젝트 감지
        // 플레이어로부터 가장 가까이 있는 오브젝트를 _interactableItem로 설정
        Collider closestColl = null;
        int collsCount = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, 0.92f, 0), 2.5f, _colls, _layerMask);
        if (collsCount > 0)
        {
            for (int i = 0; i < collsCount; i++)
            {
                // 플레이어와 오브젝트의 거리 측정
                float distance = Vector3.Distance(transform.position, _colls[i].transform.position);
                // closestColl이 null이거나 현재 오브젝트가 closestColl보다 가까운 경우 현재 인덱스의 콜라이더를 closestColl로 설정
                if (closestColl == null || distance < Vector3.Distance(transform.position, closestColl.transform.position))
                {
                    closestColl = _colls[i];
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

    #region 상호작용 : 화면 중앙에 가장 가까운 물체
    private void FindCloseInteractableItemFromRay()
    {
        // 스크린 중앙 기준 가장 가까이 있는 오브젝트를 _interactableItem로 설정
        // 레이캐스트로 중앙을 감지하고 감지된 hit 기준 거리 계산
        Collider closestColl = null;
        int collsCount = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, 0.92f, 0), 2.5f, _colls, _layerMask);
        // 레이캐스트로 중앙을 감지하고 감지된 hit 기준 거리 계산
        
        if (collsCount > 0)
        {
            for (int i = 0; i < collsCount; i++)
            {
                // 화면 중앙에서 오브젝트의 거리 측정
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;
                bool isHit = Physics.Raycast(ray, out hit, 10f);
                float distance = Vector3.Distance(hit.point, _colls[i].transform.position);
                // closestColl이 null이거나 현재 오브젝트가 closestColl보다 가까운 경우 현재 인덱스의 콜라이더를 closestColl로 설정
                if (closestColl == null || distance < Vector3.Distance(hit.point, closestColl.transform.position))
                {
                    closestColl = _colls[i];
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
        Move();
        Jump();
        Run();
        CameraLimit();
        //FindCloseInteractableItemFromPlayer();
        //FindCloseInteractableItemFromRay();
        FindCloseInteractableItemAtRay();
    }

    private void PlayerInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        _moveDir = new Vector3(x, 0, y).normalized;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _mouseInput = new Vector2(mouseX, mouseY);


        #region E 키 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 땅에 떨어진 아이템은 E누르면 즉시 상호작용
            if (PlayerManager.Instance.InteractableItem != null)
                PlayerManager.Instance.InteractableItem.Interact();
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
            PlayerManager.Instance.InteractDelay = 0f; // E 키를 떼면 딜레이 초기화
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Q)) // 'Q' 키를 눌렀을 때
        {
            SampleUIManager.Instance.ToggleInventoryUI(); // SampleUIManager의 인벤토리 토글 메서드 호출
        }


        #region 아이템 사용
        if(Input.GetMouseButtonDown(0) && PlayerManager.Instance.SelectItem != null)
        {
            // 손에 사용, 소비 아이템이 아닌 자원 아이템이 들려 있는 경우
            _animator.SetTrigger("Swing");
        }

        if (Input.GetMouseButton(0))
        {
            // 테스트용으로 마이닝 모션 실행
            _testBool = true; // 마이닝 애니메이션 실행을 위한 bool 값 설정
            // 아이템 사용은 중간에 마우스를 때면 멈춰야 하기에 코루틴이 아닌 그냥 구현
            PlayerManager.Instance.ItemDelay += Time.deltaTime;
            if(PlayerManager.Instance.ItemDelay >= 1)
            {
                //Debug.Log("아이템 사용!");
                // PlayerManager.Instance.SelectItem.Use(this.gameObject);
                PlayerManager.Instance.ItemDelay = 0f; // 아이템 사용 후 딜레이 초기화
            }
        }
        else
        {
            _testBool = false;
        }
        #endregion

        // 테스트 코드
        if (Input.GetKeyDown(KeyCode.T))
        {
            _isGrabbing = !_isGrabbing; // Grab 상태 토글
        }
    }

    #region 플레이어 이동
    private void Move()
    {
        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함
        Vector3 move = transform.TransformDirection(_moveDir) * _speed;

        // 화성이 배경이니 중력은 3.73
        _verVelocity.y -= 3.73f * Time.deltaTime;

        _controller.Move((move + _verVelocity) * Time.deltaTime);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _controller.isGrounded)
        {
            _verVelocity.y = 5f; // Jump force
        }
    }

    private void Run()
    {
        // 달리기 기능이 필요하지 않을 수도 있음.
        // 넣는다면 슬로우랑 상호작용 고려할것.
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _speed *= 2;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _speed /= 2;
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

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.92f, 0), 2.5f);

        if(_isRayHit)
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

    #region 미사용 코드
    /// <summary>
    /// 슬로우 강도를 퍼센테이지로 입력 받아 플레이어 감속
    /// </summary>
    /// <param name="percentage"></param>0~100 사이의 값으로 입력, 0은 감속 없음, 100은 정지
    //public void PlayerSlow(float percentage)
    //{
    //    _speed = _speed * (1f - percentage / 100f);
    //}
    //
    /// <summary>
    /// 슬로우의 역순으로 계산하기에 슬로우 한 퍼센테이지를 그대로 입력해야함
    /// </summary>
    /// <param name="percentage"></param>
    //public void OutOfSlow(float percentage)
    //{
    //    // 슬로우의 역순
    //    _speed = _speed / (1f - percentage / 100f);
    //}

    // overlapsphere로 교체하기에 주석처리.
    // 현재 인터렉션 방식은 한곳에 하나의 오브젝트만 있다는 걸 전제로 제작됨
    // 무조건 처음 접근한 오브젝트만 인터렉션이 가능하도록 제작
    // 여러 오브젝트가 있을 경우, 지금 방식이 아닌 다른 방식으로 코드를 작성하여야 함
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.TryGetComponent<IInteractable>(out IInteractable interact) && _interactableItem == null)
    //     {
    //         _interactableItem = interact as TestItem;
    //         TestPlayerManager.Instance.IsInIntercation = true;
    //         // 나중에 아이템과 상호작용 물체가 나뉜다고 하면
    //         // _interactableItem에 as로 넣을때 조건문을 이용하여 상황에 맞게 넣는 로직 필요
    //         // Item이라면 as Item으로, 구조물이라면 as Structure로 넣는 식으로
    //     }
    // }
    // 
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.TryGetComponent<IInteractable>(out IInteractable interact))
    //     {
    //         TestPlayerManager.Instance.IsInIntercation = false;
    //         _interactableItem = null;
    //     }
    // }
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
