using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("TestItem Interacted!");
    }
}
