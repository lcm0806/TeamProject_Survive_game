using DesignPattern;
using System.Collections;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public Vector2 MouseInput { get; private set; } // 마우스 입력
    public Vector3 MoveDir { get; set; } // 이동 방향

    public bool IsUsingTool { get; private set; } // 테스트용 bool 값, 마이닝 애니메이션 실행 여부
    public bool CanMove => !PlayerManager.Instance.Player.IsSlipping && !PlayerManager.Instance.Player.IsUsingJetPack;

    private Coroutine _itemCo;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        // 타이틀부터 플레이하는 경우
        if (SceneSystem.Instance?.GetCurrentSceneName() == SceneSystem.Instance?.GetFarmingSceneName())
        {
            PlayerInput(); // 플레이어 입력 처리
            return;
        }

        // 테스트로 사용하는 경우
        if (PlayerManager.Instance.Player != null)
            PlayerInput(); // 플레이어 입력 처리
    }

    private void PlayerInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        MouseInput = new Vector2(mouseX, mouseY);


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
        // 아이템이 사용 중이 아닐때만 교체
        if (!IsUsingTool)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayerManager.Instance.SelectItem = null;
                PlayerManager.Instance.AkimboReset();
                Destroy(PlayerManager.Instance.InHandItem);
                Destroy(PlayerManager.Instance.InHandItem2); // 아킴보 상태일 때 두번째 아이템 제거
                Destroy(PlayerManager.Instance.InHeadItem); // 머리에 착용한 아이템 제거
                                                            // 인벤토리 핫바 1번 선택
                Inventory.Instance.SelectHotbarSlot(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlayerManager.Instance.SelectItem = null;
                PlayerManager.Instance.AkimboReset();
                Destroy(PlayerManager.Instance.InHandItem);
                Destroy(PlayerManager.Instance.InHandItem2);
                Destroy(PlayerManager.Instance.InHeadItem);
                // 인벤토리 핫바 2번 선택
                Inventory.Instance.SelectHotbarSlot(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PlayerManager.Instance.SelectItem = null;
                PlayerManager.Instance.AkimboReset();
                Destroy(PlayerManager.Instance.InHandItem);
                Destroy(PlayerManager.Instance.InHandItem2);
                Destroy(PlayerManager.Instance.InHeadItem);
                // 인벤토리 핫바 3번 선택
                Inventory.Instance.SelectHotbarSlot(2);
            }
        }

        Item curItem = Inventory.Instance.GetCurrentHotbarItem();

        if (curItem == null)
        {
            PlayerManager.Instance.SelectItem = null; // 현재 핫바에 아이템이 없으면 선택 아이템을 null로 설정
            if (PlayerManager.Instance.InHandItem != null)
            {
                PlayerManager.Instance.AkimboReset();
                Destroy(PlayerManager.Instance.InHandItem); // 오브젝트 제거
                Destroy(PlayerManager.Instance.InHandItem2);
                Destroy(PlayerManager.Instance.InHeadItem);
            }
            // _testHandItem = null; // 테스트용 마인건도 null로 설정
        }
        else if (PlayerManager.Instance.SelectItem != curItem)
        {
            PlayerManager.Instance.SelectItem = curItem; // 현재 핫바에 아이템이 있으면 선택 아이템으로 설정

            if (_itemCo == null)
            {
                PlayerManager.Instance.AkimboReset();
                Destroy(PlayerManager.Instance.InHandItem); // 오브젝트 제거
                Destroy(PlayerManager.Instance.InHandItem2);
                Destroy(PlayerManager.Instance.InHeadItem);
                // 아이템이 없다면 생성 코루틴 실행
                _itemCo = StartCoroutine(ItemInstantiate());
            }
        }
        else
        {
            PlayerManager.Instance.SelectItem = curItem; // 현재 핫바에 아이템이 있으면 선택 아이템으로 설정

            if (PlayerManager.Instance.InHandItem == null)
            {
                if (_itemCo == null)
                {
                    // 아이템이 없다면 생성 코루틴 실행
                    _itemCo = StartCoroutine(ItemInstantiate());
                }
            }
        }
        #endregion

        #region 아이템 사용
        if (Input.GetMouseButtonDown(0) && PlayerManager.Instance.SelectItem as MaterialItem && !SampleUIManager.Instance.inventoryPanel.activeSelf)
        {
            // 손에 자원 아이템이 들려 있는 경우
            // _animator.SetTrigger("Swing");
        }
        else if (Input.GetMouseButton(0) && PlayerManager.Instance.SelectItem as ToolItem && !SampleUIManager.Instance.inventoryPanel.activeSelf)
        {
            IsUsingTool = true; // 마이닝 애니메이션 실행을 위한 bool 값 설정

            float itemUseRate = 0.1f;

            if (PlayerManager.Instance.IsAkimbo)
            {
                itemUseRate = 0.05f; // 아킴보 상태에서는 아이템 사용 속도를 빠르게
            }

            // 아이템 사용은 중간에 마우스를 때면 멈춰야 하기에 코루틴이 아닌 그냥 구현
            PlayerManager.Instance.ItemDelay += Time.deltaTime;
            if (PlayerManager.Instance.ItemDelay >= itemUseRate)
            {
                Debug.Log("아이템 사용!");
                PlayerManager.Instance.SelectItem?.Use(this.gameObject);
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
            IsUsingTool = false;
            PlayerManager.Instance.ItemDelay = 0f; // 마우스를 떼면 아이템 사용 딜레이 초기화
        }
        #endregion

        // 제트팩은 공중에서만 사용
        if (PlayerManager.Instance.Player != null)
            if (Input.GetKeyDown(KeyCode.LeftShift) && !PlayerManager.Instance.Player.Controller.isGrounded &&
            PlayerManager.Instance.IsUpgraded[0] && PlayerManager.Instance.AirGauge.Value > 0)
            {
                PlayerManager.Instance.Player.IsUsingJetPack = true;
                MoveDir = Vector3.zero; // 제트팩 사용시 이동 방향 초기화
            }
        if (PlayerManager.Instance.Player != null)
            if (Input.GetKeyUp(KeyCode.LeftShift) && PlayerManager.Instance.Player.IsUsingJetPack || PlayerManager.Instance.Player.Controller.isGrounded)
            {
                PlayerManager.Instance.Player.IsUsingJetPack = false;
            }

        if (!CanMove) return; // 움직일 수 없다면 이동은 생략

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        MoveDir = new Vector3(x, 0, y).normalized;
    }

    IEnumerator ItemInstantiate()
    {
        // 아이템 생성 딜레이를 위한 코루틴
        // 순서상 아이템 생성은 아이템 interact 후 각 변수들에 할당 후 이루어져야 하기에 딜레이를 추가함
        yield return new WaitForSeconds(0.01f);

        Item curItem = Inventory.Instance.GetCurrentHotbarItem();

        if (PlayerManager.Instance.IsAkimbo)
        {
            // 아킴보인 경우 양손에 툴 생성
            PlayerManager.Instance.InHandItem = Instantiate(curItem.HandleItem,
                PlayerManager.Instance.Player.PlayerRightHand.position,
                PlayerManager.Instance.Player.PlayerRightHand.rotation);
            PlayerManager.Instance.InHandItem2 = Instantiate(curItem.HandleItem,
                PlayerManager.Instance.Player.PlayerLeftHand.position,
                PlayerManager.Instance.Player.PlayerLeftHand.rotation);
            PlayerManager.Instance.InHeadItem = Instantiate(PlayerManager.Instance.SunGlasses,
                PlayerManager.Instance.Player.PlayerHead.position,
                PlayerManager.Instance.Player.PlayerHead.rotation);
        }
        else
        {
            PlayerManager.Instance.InHandItem = Instantiate(curItem.HandleItem,
            PlayerManager.Instance.Player.PlayerRightHand.position,
            PlayerManager.Instance.Player.PlayerRightHand.rotation);
        }

        _itemCo = null; // 코루틴 종료 후 null로 설정
    }
}
