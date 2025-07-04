using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class EscapeToMars : Structure
{
    public override void Interact()
    {
        ShowMessage("탈출 관련 스크립트 삽입");

    }
    private void ShowMessage(string message)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPopup(message);
        }
        else
        {
            Debug.LogWarning("UIManager가 씬에 없습니다.");
        }
    }
}
