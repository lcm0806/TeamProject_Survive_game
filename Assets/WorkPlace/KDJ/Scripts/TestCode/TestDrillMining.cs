using UnityEngine;

public class TestDrillMining : ToolAction
{
    private Collider[] _colls =  new Collider[10];
    private LayerMask _layerMask = 1 << 10;
    private bool _isMining = false;

    public override void Action(int power)
    {
        // 광물 체굴 로직
        int count = Physics.OverlapSphereNonAlloc(PlayerManager.Instance.InHandItem.transform.position 
            + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f, _colls, _layerMask);

        Debug.Log("현재 검출 레이어" + _layerMask.value);
        
        if (count > 0)
        {
            Debug.Log($"주변 광물 개수: {count}");
            _isMining = true;
            for (int i = 0; i < count; i++)
            {
                //_colls[i].GetComponent<TestOre>()?.TakeDamage(power);
                bool s = _colls[i].TryGetComponent<MineableResource>(out MineableResource ore);
                if (!s)
                {
                    continue;
                }
                ore.TakeMiningDamage(power);
            }
        }
        else
        {
            Debug.Log("주변에 광물이 없습니다.");
            _isMining = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(PlayerManager.Instance.InHandItem == null)
            return;
    
        if (_isMining)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(PlayerManager.Instance.InHandItem.transform.position 
                + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(PlayerManager.Instance.InHandItem.transform.position
                + PlayerManager.Instance.Player.transform.forward * 1.5f, 3f);
        }
    }
}
