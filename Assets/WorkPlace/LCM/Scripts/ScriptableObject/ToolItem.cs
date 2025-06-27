using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

[CreateAssetMenu(menuName = "Item/Tool Item")]
public class ToolItem : Item
{
    [Header("Tool Specific Info")]
    public float miningPower = 1f; // 채굴 도구의 채굴력 (예: 광물의 체력을 깎는 양)
    public ToolType toolType;       // 이 도구의 종류 (예: 곡괭이, 도끼, 삽 등)

    public override void Use(GameObject user)
    {
        // 부모 Item 클래스의 Use 메소드를 호출합니다.
        base.Use(user); // 디버깅 용

        Debug.Log($"{itemName}을 사용했습니다. (내구도 없음)");

        // 내구도 관련 로직이 없으므로, 여기서는 추가적인 도구별 로직은 생략하거나
        // 플레이어 컨트롤러에서 처리할 액션을 트리거하도록 합니다.
        // 예: 플레이어 컨트롤러에서 레이캐스트를 통해 채굴 가능한 대상을 찾고 공격하도록 명령
    }
}

public enum ToolType
{
    //도구의 종류 구분
    None,
    Pickaxe,
    Axe,
    Shovel
}
