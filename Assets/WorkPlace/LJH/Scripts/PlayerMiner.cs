using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMiner : MonoBehaviour
{
    private MineableResource currentTarget;
    public float interactRange = 3f;             // 상호작용 거리
    public KeyCode interactKey = KeyCode.E;      // 상호작용 키
    public LayerMask interactLayer;               // 상호작용 가능한 레이어 지정

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
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
            Debug.Log("상호작용 대상이 없습니다.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Resource"))
        {
            currentTarget = other.GetComponent<MineableResource>();
            if (currentTarget != null)
            {
                currentTarget.StartMining();
                Debug.Log("채굴 시작!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Resource"))
        {
            if (currentTarget != null)
            {
                currentTarget.StopMining();
                Debug.Log("채굴 중단!");
            }
            currentTarget = null;
        }
    }
}
