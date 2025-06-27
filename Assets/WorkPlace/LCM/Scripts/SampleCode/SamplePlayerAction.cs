using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplePlayerAction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 3f; // 상호작용 가능한 거리
    [SerializeField] private LayerMask mineableLayer; // 채굴 가능한 오브젝트 레이어 (광물 GameObject의 Layer를 설정해주세요)

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭 감지 (플레이어가 도구를 사용하려는 입력)
        // MouseButtonDown은 한 번 클릭에 한 번만 호출됩니다.
        if (Input.GetMouseButtonDown(0))
        {
            UseEquippedItem();
        }
    }

    void UseEquippedItem()
    {
        // 핫바 슬롯 선택 (예: 숫자 키 1~9)
        // PlayerController에 구현 필요
        //for (int i = 0; i < 9; i++) // 핫바 슬롯이 9개라고 가정
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Alpha1은 숫자 1 키
        //    {
        //        // Inventory 스크립트의 핫바 선택 메소드를 호출
        //        Inventory.Instance.SelectHotbarSlot(i);
        //        break;
        //    }
        //}
        Item currentItem = Inventory.Instance.GetCurrentHotbarItem();

        if (currentItem == null)
        {
            Debug.Log("핫바에 선택된 아이템이 없습니다.");
            return;
        }

        // 아이템이 ToolItem 타입인지 확인합니다.
        if (currentItem is ToolItem tool) // 'is' 연산자로 타입 확인 및 'tool' 변수로 캐스팅
        {
            // 이 도구가 채굴 도구(Pickaxe) 타입인지 확인
            if (tool.toolType == ToolType.Pickaxe)
            {
                Debug.Log($"채굴 도구 '{tool.itemName}' 사용 시도. 채굴력: {tool.miningPower}");

                // 플레이어의 시점(카메라)에서 레이캐스트 발사
                // 카메라가 플레이어의 자식 오브젝트라면 Camera.main.transform.forward 사용
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // 화면 중앙에서 레이 발사
                RaycastHit hit;

                // mineableLayer에 해당하는 오브젝트만 감지
                if (Physics.Raycast(ray, out hit, interactionRange, mineableLayer))
                {
                    SampleMineableResource mineable = hit.collider.GetComponent<SampleMineableResource>();
                    if (mineable != null)
                    {
                        Debug.Log($"{hit.collider.name}에 데미지를 줍니다.");
                        // 찾은 MineableResource에 채굴 도구의 miningPower만큼 데미지 전달
                        mineable.TakeMiningDamage(tool.miningPower);

                        // 여기에 채굴 애니메이션, 사운드, 파티클 효과 등을 추가할 수 있습니다.
                        // 예: PlayMiningAnimation();
                        // 예: PlayMiningSound();
                    }
                    else
                    {
                        Debug.Log("바라보는 오브젝트가 채굴 가능한 대상이 아닙니다.");
                    }
                }
                else
                {
                    Debug.Log("채굴할 대상을 찾지 못했습니다.");
                }
            }
            else
            {
                Debug.Log($"{tool.itemName}은 곡괭이가 아닙니다. 다른 도구 로직 필요.");
            }
        }
        else // 도구가 아닌 다른 아이템 타입 (예: 소모품, 무기)
        {
            currentItem.Use(this.gameObject); // 해당 아이템의 일반 Use 메소드 호출
        }
    }
}
