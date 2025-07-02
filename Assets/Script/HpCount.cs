using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class HpCount : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("레벨디자이너에게 : 게임 시작시 최조 체력을 initial int에 설정해주세요")]
    [Tooltip("개발팀에게 : 게임 시작 시, 이미 구현한 코드의 start에서 gameobject.HpCount.InitialHp로 초기 체력을 가져와서 이미 구현한 코드의 체력에 설정해주세요.(대문자 I입니다) 가능하면 이 스크립트는 수정하지 말아주세요")]
    [SerializeField]


    private int initialHp = 50;
    public int InitialHp
    {
        get { return initialHp; }
    }
    [Header("Emission색상을 미리 설정해둡니다. 체력에따라 바뀝니다.")]
    [Header("gameobject.HpCount.GetEmitColor(현재 체력)로 색상을 가져올 수 있습니다. 현재 체력은 이미 구현한 코드에서 가져오면 됩니다.")]
    [Tooltip("0일 때 색상")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color color0;
    [Tooltip("1~20일 때")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color color1;
    [Tooltip("21~45일 때")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color color2;
    [Tooltip("46~70일 때")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color color3;
    [Tooltip("71~일 때 색상")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color color4;

    public Color GetEmitColor(int hp)
    {
        if (hp <= 0)
        {
            return color0;
        }
        else if (hp <= 20)
        {
            return color1;
        }
        else if (hp <= 45)
        {
            return color2;
        }
        else if (hp <= 70)
        {
            return color3;
        }
        else
        {
            return color4;
        }
    }
}
