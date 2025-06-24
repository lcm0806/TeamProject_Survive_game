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
    [SerializeField] private float _baseSpeed = 5f;

    private Vector3 _verVelocity;
    private Vector3 _moveDir;
    private Vector2 _mouseInput;
    private float _totalMouseY = 0f;
    //private TestItem _interactableItem;
    private LayerMask _ignoreMask = ~(1 << 3);
    private LayerMask _layerMask = 1 << 6;
    private Collider[] _colls = new Collider[10];
    private TestItem _selectItem;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        PlayerInput();
        HandlePlayer();
    }

    // overlapsphere�� ��ü�ϱ⿡ �ּ�ó��.
    // ���� ���ͷ��� ����� �Ѱ��� �ϳ��� ������Ʈ�� �ִٴ� �� ������ ���۵�
    // ������ ó�� ������ ������Ʈ�� ���ͷ����� �����ϵ��� ����
    // ���� ������Ʈ�� ���� ���, ���� ����� �ƴ� �ٸ� ������� �ڵ带 �ۼ��Ͽ��� ��
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.TryGetComponent<IInteractable>(out IInteractable interact) && _interactableItem == null)
    //     {
    //         _interactableItem = interact as TestItem;
    //         TestPlayerManager.Instance.IsInIntercation = true;
    //         // ���߿� �����۰� ��ȣ�ۿ� ��ü�� �����ٰ� �ϸ�
    //         // _interactableItem�� as�� ������ ���ǹ��� �̿��Ͽ� ��Ȳ�� �°� �ִ� ���� �ʿ�
    //         // Item�̶�� as Item����, �������̶�� as Structure�� �ִ� ������
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
        // Ʈ���Ÿ� ������� �ʰ� overlapsphere�� ����Ͽ� �ֺ��� ���ͷ��� ������ ������Ʈ ����
        // ���� ������ �ִ� ������Ʈ�� _interactableItem�� ����
        Collider closestColl = null;
        int collsCount = Physics.OverlapSphereNonAlloc(transform.position, 2f, _colls, _layerMask);
        if (collsCount > 0)
        {
            for (int i = 0; i < collsCount; i++)
            {
                // �÷��̾�� ������Ʈ�� �Ÿ� ����
                float distance = Vector3.Distance(transform.position, _colls[i].transform.position);
                // closestColl�� null�̰ų� ���� ������Ʈ�� closestColl���� ����� ��� ���� �ε����� �ݶ��̴��� closestColl�� ����
                if (closestColl == null || distance < Vector3.Distance(transform.position, closestColl.transform.position))
                {
                    closestColl = _colls[i];
                }
            }
            // ������ closestColl�� ������ _interactableItem�� �Ҵ�
            if (closestColl != null && closestColl.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                //_interactableItem = interactable as TestItem;
                PlayerManager.Instance.InteractableItem = interactable as TestItem;
                PlayerManager.Instance.IsInIntercation = true;
                // ���߿� �����۰� ��ȣ�ۿ� ��ü�� �����ٰ� �ϸ�
                // _interactableItem�� as�� ������ ���ǹ��� �̿��Ͽ� ��Ȳ�� �°� �ִ� ���� �ʿ�
                // Item�̶�� as Item����, �������̶�� as Structure�� �ִ� ������
            }
        }
        else
        {
            // �ֺ��� ���ͷ��� ������ ������Ʈ�� ������ _interactableItem�� null�� ����
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
        // �׽�Ʈ�� ���콺 �����
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
        // ī�޶� �������� ������ ��� �����̵��� �����ؾ���
        Vector3 move = transform.TransformDirection(_moveDir) * _speed;

        // ȭ���� ����̴� �߷��� 3.73
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

        if(Input.GetKeyDown(KeyCode.E) && PlayerManager.Instance.InteractableItem != null)
        {
            PlayerManager.Instance.InteractableItem.Interact();
        }

        if(Input.GetMouseButtonDown(0) && _selectItem != null)
        {
            // ������ ��� ������ �����ȴٸ�, ���. ������ �׽�Ʈ�� Interact �޼��� ȣ��
            //_selectItem.Interact();
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
        // �޸��� ����� �ʿ����� ���� ���� ����.
        // �ִ´ٸ� ���ο�� ��ȣ�ۿ� �����Ұ�.
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
        // ī�޶� ���� �ε����� ��� �� ��ġ��ŭ ������ �̵�
        // �࿡�� ī�޶�� ���̸� �߻��Ͽ� ���� �ε����� ���, �ε����� ��ġ�� ����Ͽ� ī�޶� �̵���Ŵ
        RaycastHit hit;
        if (Physics.Raycast(_virCamAxis.position, -_virCamAxis.forward, out hit, 4.3f, _ignoreMask))
        {
            Vector3 targetPos = hit.point + _virCam.transform.forward * 0.3f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, targetPos, 0.5f);
        }
        else
        {
            // ���� �ε����� �ʴ� ��� ��ġ ����
            Vector3 resetPos = _virCamAxis.position - _virCamAxis.forward * 4f;
            _virCam.transform.position = Vector3.Lerp(_virCam.transform.position, resetPos, 0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        // Gizmos�� ����Ͽ� ���� ǥ��
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2.5f);
        //Gizmos.DrawLine(_virCamAxis.position, _virCamAxis.position - _virCamAxis.forward * 4f);
    }

    /// <summary>
    /// ���ο� ������ �ۼ��������� �Է� �޾� �÷��̾� ����
    /// </summary>
    /// <param name="percentage"></param>0~100 ������ ������ �Է�, 0�� ���� ����, 100�� ����
    public void PlayerSlow(float percentage)
    {
        _speed = _speed * (1f - percentage / 100f);
    }

    /// <summary>
    /// ���ο��� �������� ����ϱ⿡ ���ο� �� �ۼ��������� �״�� �Է��ؾ���
    /// </summary>
    /// <param name="percentage"></param>
    public void OutOfSlow(float percentage)
    {
        // ���ο��� ����
        _speed = _speed / (1f - percentage / 100f);
    }
}