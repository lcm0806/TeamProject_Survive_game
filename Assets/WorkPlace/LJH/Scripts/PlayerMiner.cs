using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMiner : MonoBehaviour
{
    private MineableResource currentTarget;
    public float interactRange = 3f;             // 상호작용 거리
    public KeyCode interactKey = KeyCode.E;      // 상호작용 키
    public LayerMask interactLayer;               // 상호작용 가능한 레이어 지정

    [SerializeField] public float miningRange = 6f;
    [SerializeField] public float miningDamagePerSecond = 1f;
    

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
        if (Input.GetMouseButton(0))  //좌클릭 누르고 있는 동안
        {
            TryMine();
        }
        else
        {
            currentTarget = null;
        }
    }

    private void TryInteract()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            // 인터페이스로 가져오기
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log("상호작용 가능한 오브젝트가 아닙니다.");
            }
        }
        else
        {
            Debug.Log("상호작용 가능한 보급상자가 없습니다.");
        }
    }
   
    void TryMine()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDir = transform.forward;

        //레이캐스트 시각화 (씬 뷰에서만 보임)
        Debug.DrawRay(rayOrigin, rayDir * miningRange, Color.red, 0.1f);

        Ray ray = new Ray(rayOrigin, rayDir);
        if (Physics.Raycast(ray, out RaycastHit hit, miningRange))
        {
            if (hit.collider.CompareTag("Resource"))
            {
                MineableResource resource = hit.collider.GetComponent<MineableResource>();
                if (resource != null)
                {
                    currentTarget = resource;
                    resource.TakeMiningDamage(miningDamagePerSecond * Time.deltaTime);
                }
            }
        }
        else
        {
            Debug.Log("상호작용 가능한 광물이 없습니다.");
        }
    }
}
