using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractableItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("TestItem Interacted!");
    }
}
