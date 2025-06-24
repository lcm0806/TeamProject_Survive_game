using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class TestPlayerManager : Singleton<TestPlayerManager>
{
    public bool IsInIntercation = false;
    public TestItem InteractableItem { get; set; }

    private void Awake()
    {
        SingletonInit();
    }
}
