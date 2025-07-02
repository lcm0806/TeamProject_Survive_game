using UnityEngine;

public class TPSController : MonoBehaviour
{
    // 이동 및 회전 속도
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 200.0f;

    // 카메라 상하 회전을 위한 변수
    public Transform cameraHolder;
    private float verticalLookRotation;

    // ★★★ 중력 관련 변수 추가 ★★★
    public float jumpSpeed = 8.0f; // 점프 속도 (클래스 상단 변수 영역에 추가)
    public float gravity = 9.81f; // 중력값
    private float yVelocity = 0;  // 캐릭터의 수직 속도

    // 필수 컴포넌트
    private CharacterController charController;

    void Awake()
    {
        charController = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. 플레이어 회전 (마우스 좌우)
        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        // 2. 카메라 회전 (마우스 상하)
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 60f);
        cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);

        // 3. 플레이어 이동 (WASD)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(h, 0, v);
        // 이동 방향을 플레이어가 바라보는 방향 기준으로 변환
        moveDirection = transform.TransformDirection(moveDirection);

        // ★★★ 중력 로직 시작 ★★★

        // isGrounded는 CharacterController가 땅에 닿아있는지 여부를 알려줌
        if (charController.isGrounded)
        {
            yVelocity = -1.0f;

            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpSpeed;
            }
        }
        else
        {
            yVelocity -= gravity * Time.deltaTime;  // 중력 적용
        }

        // 수직 속도(yVelocity)를 이동 방향의 y값에 적용
        moveDirection.y = yVelocity;

        // ★★★ 중력 로직 끝 ★★★
        // ★★★ 점프 기능 추가 ★★★


        // 중력 로직 이후, charController.Move() 이전에 추가할 코드:
        // 점프 입력 감지 (스페이스바)
        if (Input.GetButtonDown("Jump") && charController.isGrounded)
        {
            // 땅에 있을 때만 점프 가능
            // yVelocity에 점프 속도를 더해줌
            yVelocity = jumpSpeed;
        }
        // ★★★ 점프 기능 끝 ★★★
        
      
// 최종적으로 계산된 방향과 속도로 캐릭터를 이동시킴
// 이제 Y축 움직임(중력)이 포함되어 있음
charController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}