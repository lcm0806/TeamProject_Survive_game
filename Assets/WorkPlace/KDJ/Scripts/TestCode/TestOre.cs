using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOre : MonoBehaviour
{
    private float hp;

    private void Awake()
    {
        hp = 100;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log($"{gameObject.name}이(가) {damage}의 피해를 받았습니다. 남은 HP: {hp}");
    }
}
