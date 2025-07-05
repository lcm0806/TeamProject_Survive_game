using UnityEngine;

public class RocketInteractionTrigger : Structure
{
    public Rocket rocketDialogue; // 혹시 쓸 일이 있을 경우
    public Item finalItem;        // 엔딩 아이템으로 사용할 ScriptableObject

    public override void Interact()
    {
        // UI 열기
        DayScriptSystem.Instance.ShowDialoguse();

        // 인벤토리에 엔딩 아이템 있는지 확인
        bool hasFinalItem = InventorySystem.Instance.HasItem(finalItem);

        if (!GameState.Instance.HasFoundRocket) // 최초 발견
        {
            GameState.Instance.HasFoundRocket = true;

            DayScriptSystem.Instance.SetDialogue(
                DayScriptSystem.Instance.TriggerFirstSpaceshipScene()
            );
        }
        else if (!hasFinalItem) // 아이템 없음
        {
            DayScriptSystem.Instance.SetDialogue(
                DayScriptSystem.Instance.TriggerSpaceshipDeniedEvent()
            );
        }
        else if (!GameState.Instance.HasStartedEnding) // 아이템 있음 + 엔딩 아직 시작 안함
        {
            GameState.Instance.HasStartedEnding = true;

            DayScriptSystem.Instance.SetDialogue(
                DayScriptSystem.Instance.StartEndingSequence()
            );

            // 여기서 아이템 제거 로직도 추가 가능
            // InventorySystem.Instance.RemoveItem(finalItem, 1); 
        }
        else // 마지막 엔딩 대사
        {
            DayScriptSystem.Instance.SetDialogue(
                DayScriptSystem.Instance.EndingScript()
            );
        }
    }
    //private IEnumerator WaitForMouseClick()
    //{
    //    yield return null; // 1프레임 대기 (동일 프레임 클릭 무시)

    //    // 마우스 버튼이 눌릴 때까지 대기
    //    while (!Input.GetMouseButtonDown(0))
    //        yield return null;
        
    //    Time.timeScale = 1f;
    //    Cursor.lockState = CursorLockMode.Locked;
    //    Cursor.visible = false;

    //    popupActive = false;
    //}

    //private void ShowMessage(string message)
    //{
    //    if (UIManager.Instance != null)
    //    {
    //        UIManager.Instance.ShowPopup(message);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("UIManager가 씬에 없습니다.");
    //    }
    //}
}
