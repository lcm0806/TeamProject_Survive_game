using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public class RocketInteractionTrigger : Structure
{
    public Rocket rocketDialogue;
    string message;
    public override void Interact()
    {

        // 대화 UI 켜고 첫 줄 표시
        //rocketDialogue.gameObject.SetActive(true);   // RenpyCanvas도 켜짐

        //UIManager.Instance.HidePopup();

        // 플레이어 이동/공격 잠금
        //InputManager.Instance.LockPlayerControl(true);

        // 게임 일시 정지
        //Time.timeScale = 0f;

        ShowMessage(message);

        // 커서 활성(PC라면)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        enabled = false;               // 재진입 방지
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
