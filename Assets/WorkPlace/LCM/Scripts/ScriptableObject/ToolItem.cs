using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

[CreateAssetMenu(menuName = "Item/Tool Item")]
public class ToolItem : Item
{
    [Header("Tool Specific Info")]

    public float miningPower = 1f; // 채굴 도구의 채굴력 (예: 광물의 체력을 깎는 양)
    public ToolType toolType;       // 이 도구의 종류 (예: 곡괭이, 도끼, 삽 등)
    public GameObject toolPrefab;     // 플레이어 손에 들릴 오브젝트 프리팹
    public GameObject toolObject;     // 도구의 실제 오브젝트 (예: 곡괭이, 도끼 등)
    public ToolAction toolAction;

    public override void Use(GameObject user)
    {
        // 부모 Item 클래스의 Use 메소드를 호출합니다.
        /*base.Use(user);*/ // 디버깅 용
        toolAction.Action((int)miningPower);

        Debug.Log($"{itemName}占쏙옙 占쏙옙占쏙옙颯占쏙옙求占. (占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙)");

    }
}

public enum ToolType
{
    //占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쏙옙
    None,
    Pickaxe,
    Axe,
    Shovel
}
