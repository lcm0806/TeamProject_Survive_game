using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class TestPlayerManager : Singleton<TestPlayerManager>
{
    public bool IsInIntercation = false;

    private void Awake()
    {
        SingletonInit();
    }
}