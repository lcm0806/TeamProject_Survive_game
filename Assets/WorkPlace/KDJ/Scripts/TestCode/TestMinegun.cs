using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestMinegun : ToolItem
{
    private bool _isMining;

    private TestDrillMining _drillMining;

    public override void Use(GameObject user)
    {
        Debug.Log($"들고 있는 아이템 : {PlayerManager.Instance.Player._testHandItem.name}");
        if (_drillMining == null) _drillMining = PlayerManager.Instance.Player._testHandItem.GetComponent<TestDrillMining>();
        _drillMining.Mining((int)miningPower);
    }
}
