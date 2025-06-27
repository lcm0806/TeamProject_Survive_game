using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

namespace Test
{
    [CreateAssetMenu(menuName = "Item/Test Tool Item")]
    public class TestToolItem : Item
    {
        [Header("Tool Specific Info")]
        public float miningPower = 1f; // 채굴 도구의 채굴력 (예: 광물의 체력을 깎는 양)
        public ToolType toolType;       // 이 도구의 종류 (예: 곡괭이, 도끼, 삽 등) 
        public GameObject toolPrefab;     // 플레이어 손에 들릴 오브젝트
        public TestToolAction toolAction; // 도구의 행동을 정의하는 스크립트

        public override void Use(GameObject user)
        {
            // 부모 Item 클래스의 Use 메소드를 호출합니다.
            //base.Use(user); // 디버깅 용
            toolAction.Action((int)miningPower);
            Debug.Log($"{itemName}을 사용했습니다. (내구도 없음)");
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
}
