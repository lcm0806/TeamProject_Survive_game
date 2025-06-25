using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private CinemachineVirtualCamera _virCam;
    [SerializeField] private Transform _virCamAxis;

    [Header("Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;
    [SerializeField] private float _speed;
    [SerializeField] private float _baseSpeed = 5f;

    private Vector3 _verVelocity;
    private Vector3 _moveDir;
    private Vector2 _mouseInput;
    private float _totalMouseY = 0f;
    private LayerMask _ignoreMask = ~(1 << 3);
    private LayerMask _layerMask = 1 << 6;
    private Collider[] _colls = new Collider[10];
    private Item _selectItem;
    private WorldItem _worldItem;
    private Coroutine _itemUseCoroutine;
    private bool _canUseItem => _itemUseCoroutine == null;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        PlayerInput();
        HandlePlayer();
    }

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

    private void FindInteractableItem()
    {
        // 트리거를 사용하지 않고 overlapsphere를 사용하여 주변의 인터렉션 가능한 오브젝트 감지
        // 가장 가까이 있는 오브젝트를 _interactableItem로 설정
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
                //_interactableItem = interactable as TestItem;
                PlayerManager.Instance.InteractableItem = interactable as WorldItem;
                PlayerManager.Instance.IsInIntercation = true;
                // 나중에 아이템과 상호작용 물체가 나뉜다고 하면
                // _interactableItem에 as로 넣을때 조건문을 이용하여 상황에 맞게 넣는 로직 필요
                // Item이라면 as Item으로, 구조물이라면 as Structure로 넣는 식으로
            }
        }
        else
        {
            // 주변에 인터렉션 가능한 오브젝트가 없으면 _interactableItem을 null로 설정
            if (PlayerManager.Instance.InteractableItem != null)
            {
                //_interactableItem = null;
                PlayerManager.Instance.InteractableItem = null;
                PlayerManager.Instance.IsInIntercation = false;
            }
        }
    }

    private void Init()
    {
        // 테스트용 마우스 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _speed = _baseSpeed;
    }

    private void HandlePlayer()
    {
        AimControl();
        Move();
        Jump();
        Run();
        CameraLimit();
        FindInteractableItem();
    }

    private void Move()
    {
        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함
        Vector3 move = transform.TransformDirection(_moveDir) * _speed;

        // 화성이 배경이니 중력은 3.73
        _verVelocity.y -= 3.73f * Time.deltaTime;

        _controller.Move((move + _verVelocity) * Time.deltaTime);
    }

    private void PlayerInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        _moveDir = new Vector3(x, 0, y).normalized;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _mouseInput = new Vector2(mouseX, mouseY);

        if (Input.GetKeyDown(KeyCode.E) && PlayerManager.Instance.InteractableItem != null)
        {
            PlayerManager.Instance.InteractableItem.Interact();
        }

        // 아이템을 마우스 좌클릭 하면 사용. 누르고 있는동안 주기적으로 계속 사용
        if (Input.GetMouseButton(0) && _selectItem != null)
        {
            // 아이템 사용은 아이템 파트에서 제작 되면 세부 구현
            // 일단은 아이템을 연속으로 사용하는 코루틴을 먼저 구현하기
            if (_canUseItem)
            {
                _itemUseCoroutine = StartCoroutine(ItemUsing());
            }

            // 소비 아이템의 경우 꾹 눌렀을때 사용되도록 설정해달라고 요청받음
            // 해당 부분 구현 필요
        }
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

    IEnumerator ItemUsing()
    {
        // 아이템 연속 사용 코루틴
        // 아이템 사용간의 딜레이 적용
        // 아래는 임시
        _selectItem.Use(this.gameObject);
        yield return new WaitForSeconds(1f);
        _itemUseCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.92f, 0), 2.5f);
    }

    // 미사용. 하지만 혹시 몰라 남겨놓음
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
}
