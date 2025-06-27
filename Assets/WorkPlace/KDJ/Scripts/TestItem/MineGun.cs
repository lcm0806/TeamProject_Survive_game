using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MineGun : Structure
{
    public override void Interact()
    {
        PlayerManager.Instance.SelectItem = ScriptableObject.CreateInstance<TestMinegun>();
        gameObject.SetActive(false);
    }
}
