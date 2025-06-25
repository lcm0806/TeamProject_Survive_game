using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Item/Material Item")]
public class MaterialItem : Item
{
    //희귀도 같은 공통적인 속성 추가시 이곳에

    public override void Use(GameObject user)
    {
        Debug.Log($"{itemName}은 직접 사용할수 없습니다.");
    }
}
