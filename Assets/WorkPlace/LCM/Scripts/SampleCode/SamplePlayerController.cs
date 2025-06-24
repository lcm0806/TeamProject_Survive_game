using Cinemachine;
using UnityEditor;
using UnityEngine;

public class SamplePlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private CinemachineVirtualCamera _virCam;
    [SerializeField] private Transform _virCamAxis;

    [Header("Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;
    [SerializeField] private float _speed;

    private Vector3 _verVelocity;
    private Vector3 _moveDir;
    private Vector2 _mouseInput;
    private float _totalMouseY = 0f;
    private WorldItem _interactableItem;
    private LayerMask _ignoreMask = ~(1 << 3);

    [Header("UI")]
    [SerializeField] private GameObject _inventoryPanel; // 인벤토리 UI 패널 GameObject
    private bool _isInventoryOpen = false; // 인벤토리 열림 상태

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        PlayerInput();
        HandlePlayer();
    }

    // 현재 인터렉션 방식은 한곳에 하나의 오브젝트만 있다는 걸 전제로 제작됨
    // 무조건 처음 접근한 오브젝트만 인터렉션이 가능하도록 제작
    // 여러 오브젝트가 있을 경우, 지금 방식이 아닌 다른 방식으로 코드를 작성하여야 함
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out IInteractable interact) && _interactableItem == null)
        {
            _interactableItem = interact as WorldItem;
            TestPlayerManager.Instance.IsInIntercation = true;
            // 나중에 아이템과 상호작용 물체가 나뉜다고 하면
            // _interactableItem에 as로 넣을때 조건문을 이용하여 상황에 맞게 넣는 로직 필요
            // Item이라면 as Item으로, 구조물이라면 as Structure로 넣는 식으로
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out IInteractable interact))
        {
            TestPlayerManager.Instance.IsInIntercation = false;
            _interactableItem = null;
        }
    }

    private void Init()
    {
        // 테스트용 마우스 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandlePlayer()
    {
        AimControl();
        Move();
        Jump();
        Run();
        CameraLimit();
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

        if (Input.GetKeyDown(KeyCode.E) && _interactableItem != null)
        {
            _interactableItem.Interact();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryUI();
        }
    }

    private void ToggleInventoryUI()
    {
        if (_inventoryPanel == null) return;

        _isInventoryOpen = !_isInventoryOpen; // 상태 반전
        _inventoryPanel.SetActive(_isInventoryOpen); // 패널 활성화/비활성화

        Debug.Log($"인벤토리 상태: {(_isInventoryOpen ? "열림" : "닫힘")}");

        // 인벤토리 열림/닫힘에 따라 마우스 커서 상태 및 플레이어 움직임 제어
        if (_isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
            Cursor.visible = true; // 커서 보이게
            _controller.enabled = false; 
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
            Cursor.visible = false; // 커서 숨김
            _controller.enabled = true; 
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
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speed = 10f;
        }
        else
        {
            _speed = 5f;
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
        if (Physics.Raycast(_virCamAxis.position, -_virCamAxis.forward, out hit, 4.3f, _ignoreMask))
        {
            Vector3 targetPos = hit.point + _virCam.transform.forward * 0.3f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, targetPos, 0.5f);
        }
        else
        {
            // 벽에 부딪히지 않는 경우 위치 리셋
            Vector3 resetPos = _virCamAxis.position - _virCamAxis.forward * 4f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, resetPos, 0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_virCamAxis.position, _virCamAxis.position - _virCamAxis.forward * 4f);
    }
}
