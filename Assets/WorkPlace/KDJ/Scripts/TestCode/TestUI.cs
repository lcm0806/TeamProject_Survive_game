using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class TestUI : MonoBehaviour
{
    [SerializeField] private GameObject _interactUI;

    private ObseravableProperty<bool> _isInInteract = new();

    private void Awake()
    {
        _isInInteract.Subscribe(SetUI);
    }

    private void Update()
    {
        _isInInteract.Value = TestPlayerManager.Instance.IsInIntercation;
    }

    private void OnDestroy()
    {
        _isInInteract.Unsubscribe(SetUI);
    }

    private void SetUI(bool value)
    {
        if (_isInInteract.Value)
        {
            _interactUI.SetActive(true);
        }
        else
        {
            _interactUI.SetActive(false);
        }
    }
}
