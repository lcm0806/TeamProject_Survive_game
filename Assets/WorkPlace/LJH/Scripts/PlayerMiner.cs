using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMiner : MonoBehaviour
{
    private MineableResource currentTarget;
    public float interactRange = 3f;             // ��ȣ�ۿ� �Ÿ�
    public KeyCode interactKey = KeyCode.E;      // ��ȣ�ۿ� Ű
    public LayerMask interactLayer;               // ��ȣ�ۿ� ������ ���̾� ����

    [SerializeField] public float miningRange = 6f;
    [SerializeField] public float miningDamagePerSecond = 1f;
    

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
        if (Input.GetMouseButton(0))  //��Ŭ�� ������ �ִ� ����
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
            // �������̽��� ��������
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log("��ȣ�ۿ� ������ ������Ʈ�� �ƴմϴ�.");
            }
        }
        else
        {
            Debug.Log("��ȣ�ۿ� ������ ���޻��ڰ� �����ϴ�.");
        }
    }
   
    void TryMine()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDir = transform.forward;

        //����ĳ��Ʈ �ð�ȭ (�� �信���� ����)
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
            Debug.Log("��ȣ�ۿ� ������ ������ �����ϴ�.");
        }
    }
}
