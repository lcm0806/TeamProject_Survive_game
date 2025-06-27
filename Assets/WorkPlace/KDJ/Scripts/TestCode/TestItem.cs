using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class TestItem : ScriptableObject
{
    [Header("Basic Item Info")]
    public string itemName = "New Item"; //아이템 이름

    [TextArea]
    public string description = "A basic item."; // 아이템 설명

    public Sprite icon;

    [Header("World Representation")]
    public GameObject WorldPrefab;


    [Header("If the item can be equipped")]
    public GameObject equipmentPrefab;

    [Header("Stacking")]
    public bool isStackable = false; // 이 아이템이 스택 가능한지 여부
    public int maxStackSize = 99;

    public virtual void Use(GameObject user)
    {
        Debug.Log($"{user.name} Used {itemName}");
    }
}
