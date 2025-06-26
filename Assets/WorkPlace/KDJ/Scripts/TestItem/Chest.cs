using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Structure
{
    public override void Interact()
    {
        Debug.Log("상자가 비었습니다!");
    }
}
