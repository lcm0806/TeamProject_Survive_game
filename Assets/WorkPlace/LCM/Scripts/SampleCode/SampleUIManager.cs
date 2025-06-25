using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;
using System;
using TMPro;

public class SampleUIManager : Singleton<SampleUIManager>
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;

    public event Action<bool> OnInventoryUIToggled;

    private void Awake()
    {
        SingletonInit();
        inventoryPanel.SetActive(false);

        SetItemDescription("");
    }
    public void ToggleInventoryUI()
    {
        bool currentStatus = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(currentStatus);

        OnInventoryUIToggled?.Invoke(currentStatus);

        // 커서 및 플레이어 제어 로직
        if (currentStatus)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            // PlayerController.Instance.SetCanMove(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // PlayerController.Instance.SetCanMove(true);
        }
    }

    public void SetItemDescription(string description)
    {
        if (_itemDescriptionText != null)
        {
            _itemDescriptionText.text = description;
        }
    }
}
