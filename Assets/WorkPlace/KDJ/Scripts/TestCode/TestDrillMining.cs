using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class TestDrillMining : MonoBehaviour
{
    private Collider[] _colls =  new Collider[10];
    private LayerMask _layerMask = 1 << 8;
    private bool _isMining = false;

    public void Mining(int power)
    {
        // 광물 체굴 로직
        int count = Physics.OverlapSphereNonAlloc(transform.position + PlayerManager.Instance.Player.transform.forward, 4f, _colls, _layerMask);

        if (count > 0)
        {
            _isMining = true;
            for (int i = 0; i < count; i++)
            {
                _colls[i].GetComponent<TestOre>()?.TakeDamage(power);
                Debug.Log($"{_colls[i].gameObject.name}에게 {power}의 피해를 입혔습니다.");
            }
        }
        else
        {
            Debug.Log("주변에 광물이 없습니다.");
            _isMining = false;
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if(PlayerManager.Instance.Player._testHandItem == null)
    //         return;
    // 
    //     if (_isMining)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireSphere(transform.position + PlayerManager.Instance.Player.transform.forward, 4f);
    //     }
    //     else
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawWireSphere(transform.position + PlayerManager.Instance.Player.transform.forward, 4f);
    //     }
    // }
}
