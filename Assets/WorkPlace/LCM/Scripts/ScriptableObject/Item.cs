using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Item Info")]
    public string itemName = "New Item"; //아이템 이름

    [TextArea]
    public string description = "A basic item."; // 아이템 설명

    public Sprite icon;
    public SlotTag itemTag = SlotTag.None;

    [Header("World Representation")]
    public GameObject WorldPrefab;


    [Header("If the item can be equipped")]
    public GameObject equipmentPrefab;

    public virtual void Use(GameObject user)
    {
        Debug.Log($"{user.name} Used {itemName}");
    }
}
