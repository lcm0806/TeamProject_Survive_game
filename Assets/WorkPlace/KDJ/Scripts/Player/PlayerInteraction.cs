using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private LayerMask _layerMask = 1 << 6;
    private RaycastHit _rayHit;
    private Vector3 _rayEndPos;
    private Collider[] _colls = new Collider[10];

    public Vector3 GroundNormal { get; private set; } = Vector3.up; // 땅의 법선 벡터
    public Collider CurHitColl { get; private set; } // 현재 충돌한 콜라이더
    public Collider LastHitColl { get; private set; } // 마지막으로 충돌한 콜라이더
    public float GroundCos { get; private set; } // 땅의 법선 벡터와 Y축의 코사인 값
    public float SlopeCos { get; private set; } // 플레이어의 슬로프 제한 각도에 대한 코사인 값
    public bool IsRayHit { get; private set; } // 레이캐스트가 성공했는지 여부


    private void Start()
    {
        Init();
    }

    private void OnDrawGizmos()
    {
        // Gizmos를 사용하여 레이 표시
        Gizmos.color = Color.green;
        //Gizmos.DrawWireSphere(transform.position + new Vector3(0, 0.92f, 0), 2.5f);

        if (IsRayHit)
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
        SlopeCos = Mathf.Cos(PlayerManager.Instance.Player.Controller.slopeLimit * Mathf.Deg2Rad);
    }

    private void Update()
    {
        FindCloseInteractableItemAtRay();
    }

    private void LateUpdate()
    {
        OnControllerColliderExit(); // LateUpdate에서 콜라이더가 닿지 않는 경우를 처리
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CurHitColl = hit.collider; // 현재 충돌한 콜라이더를 저장
        GroundNormal = hit.normal; // 충돌한 표면의 법선 벡터를 저장
        GroundCos = Vector3.Dot(hit.normal, Vector3.up);
    }

    private void OnControllerColliderExit()
    {
        // 콜라이더에 닿았었는데 이제는 닿지 않는 경우
        if (LastHitColl != null && CurHitColl == null)
        {
            // 움직임을 고정 방향으로 설정
            PlayerManager.Instance.Player.FixedDir = transform.TransformDirection(InputManager.Instance.MoveDir) * PlayerManager.Instance.Speed * 0.5f;
        }

        LastHitColl = CurHitColl; // 현재 콜라이더를 마지막 콜라이더로 저장
        CurHitColl = null; // 현재 콜라이더를 null로 초기화
    }

    public void FindCloseInteractableItemAtRay()
    {
        // overlapsphere를 플레이어 위치가 아닌 레이의 끝지점에서 생성
        // 스크린 중앙 기준 가장 가까이 있는 오브젝트를 _interactableItem로 설정
        // 레이캐스트로 중앙을 감지하고 감지된 hit 기준 거리 계산
        Collider closestColl = null;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        IsRayHit = Physics.Raycast(ray, out _rayHit, 6f);
        int collsCount = 0;
        Gizmos.color = Color.green;

        if (IsRayHit)
        {
            // 레이캐스트가 성공하면 hit.point를 기준으로 overlapsphere를 생성
            collsCount = Physics.OverlapSphereNonAlloc(_rayHit.point, 2.5f, _colls, _layerMask);
        }
        else
        {
            // 실패시 카메라의 위치에서 레이 방향으로 6f 떨어진 지점에서 overlapsphere를 생성
            _rayEndPos = PlayerManager.Instance.Player.VirCamAxis.position + Camera.main.transform.forward * 2f;
            collsCount = Physics.OverlapSphereNonAlloc(_rayEndPos, 2.5f, _colls, _layerMask);
        }

        if (collsCount > 0)
        {
            for (int i = 0; i < collsCount; i++)
            {
                if (IsRayHit)
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
}
