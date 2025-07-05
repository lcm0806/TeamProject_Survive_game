using UnityEngine;

public class RocketInteractionTrigger : Structure
{
    public Rocket rocketDialogue;
    public Item requiredFinalItem; // 인스펙터에서 세팅할 최종 제작 아이템

    public override void Interact()
    {
        // 대화 시작 시 다이얼로그 UI 활성화
        DayScriptSystem.Instance.ShowDialoguse();

        if (Storage.Instance.HasItem(requiredFinalItem))
        {
            // 엔딩 아이템이 있을 때 엔딩으로
            DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.StartEndingSequence());
        }
        else
        {
            // 없을 때 제작 정보 대사 출력
            DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.TriggerSpaceshipDeniedEvent());
        }

        // 로켓 UI도 보여주려면 아래도 활성화
        rocketDialogue.gameObject.SetActive(true);
    }
}
    

