using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed;
    [SerializeField] private CinemachineVirtualCamera _virCam;

    [Header("Mouse Config")]
    [SerializeField][Range(0, 5)] private float _mouseSensitivity = 1;

    private Vector3 _verVelocity;

    private void Awake()
    {
        Init();
    }


    private void Update()
    {
        POVControl();
        Move(PlayerMoveInput());
        Jump();
        Run();
    }

    private void Init()
    {
        // 테스트용 마우스 숨기기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move(Vector3 moveDir)
    {
        // 카메라를 기준으로 정면을 잡고 움직이도록 수정해야함
        Vector3 move = transform.TransformDirection(moveDir) * _speed;

        _verVelocity.y -= 3.73f * Time.deltaTime;

        _controller.Move((move + _verVelocity) * Time.deltaTime);
    }

    private Vector3 PlayerMoveInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        return new Vector3(x, 0, y).normalized;
    }

    private Vector2 PlayerMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _verVelocity.y = 5f; // Jump force
        }
    }

    private void Run()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speed = 10f;
        }
        else
        {
            _speed = 3f;
        }
    }

    private void POVControl()
    {
        float mouseX = PlayerMouseInput().x * _mouseSensitivity;
        float mouseY = PlayerMouseInput().y * _mouseSensitivity;

        float clampedMouseY = _virCam.transform.localEulerAngles.x - mouseY;
        if (clampedMouseY > 180)
        {
            clampedMouseY -= 360;
        }

        _virCam.transform.localRotation = Quaternion.Euler(clampedMouseY, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}
