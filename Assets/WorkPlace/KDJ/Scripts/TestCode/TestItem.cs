using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestItem : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log($"Interact with {gameObject.name}");
    }

    // public void UseItem();
}
