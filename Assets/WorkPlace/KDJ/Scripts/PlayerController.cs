using Cinemachine;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private TestItem _interactableItem;

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
            _interactableItem = interact as TestItem;
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

        if(Input.GetKeyDown(KeyCode.E) && _interactableItem != null)
        {
            _interactableItem.Interact();
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
        _totalMouseY = Mathf.Clamp(_totalMouseY - mouseInput.y, -50, 50);

        float clampedMouseY = _virCam.transform.localEulerAngles.x - mouseInput.y;
        if (clampedMouseY > 180)
        {
            clampedMouseY -= 360;
        }

        // 가능하면 y축 시점이 일정 각도를 넘어갔을때 카메라도 회전만 하는게 아닌 위치가 이동하도록 수정
        // 카메라가 상하로 과도한 각도로 회전하는 경우 카메라는 일정 부분만 이동하고 그 이후론 축을 회전
        if (_totalMouseY > 30)
        {
            _virCamAxis.transform.localRotation = Quaternion.Euler(_totalMouseY - 30, 0, 0);
        }
        else if (_totalMouseY < -30)
        {
            _virCamAxis.transform.localRotation = Quaternion.Euler(_totalMouseY + 30, 0, 0);
        }
        else
        {
            _virCam.transform.localRotation = Quaternion.Euler(clampedMouseY, 0, 0);
        }

        transform.Rotate(Vector3.up * mouseInput.x);
    }

    private void CameraLimit()
    {
        // 카메라가 벽에 부딪히는 경우 벽 위치만큼 앞으로 이동
        // 축에서 카메라로 레이를 발사하여 벽에 부딪히는 경우, 부딛히는 위치를 계산하여 카메라를 이동시킴
        RaycastHit hit;
        if (Physics.Raycast(_virCamAxis.position, (_virCam.transform.position - _virCamAxis.position).normalized, out hit, 9.3f))
        {
            Vector3 targetPos = hit.point + _virCam.transform.forward * 0.3f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, targetPos, 0.5f); // 카메라가 벽에 너무 붙지 않도록 약간의 오프셋 추가
        }
        else
        {
            Vector3 resetPos = _virCamAxis.position - _virCamAxis.forward * 9f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, resetPos, 0.5f); // 벽에 부딪히지 않는 경우 기본 위치로 이동
        }
    }

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.red; // 레이 색상 설정
        Gizmos.DrawLine(_virCamAxis.position, _virCamAxis.position - _virCamAxis.forward * 9f); // 레이 표시용
    }
}
