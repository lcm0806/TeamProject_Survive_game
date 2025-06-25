using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class SamplePlayerManager : Singleton<SamplePlayerManager>
{
    public bool IsInIntercation = false;
    public WorldItem InteractableItem { get; set; }

    private void Awake()
    {
        SingletonInit();
    }
}
