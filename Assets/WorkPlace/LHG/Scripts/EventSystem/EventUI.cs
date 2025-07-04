using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    //패널, 버튼, 스크롤 뷰 등 ui연동 참조 직렬화

    [Header("mainUI 완료불가, 완료가능, 완료됨")]
    public Button CanNotCompleteBtns;
    public Button CanCompleteBtns;
    public Button Completed;

    [Header("mainUI 이벤트 타이틀, 설명, 효과, 완료조건")]
    public TMP_Text mainUIEventTitle;
    public TMP_Text mainUIEventDesc;
    public TMP_Text mainUIEventEffectName; //두가지 이상인경우?
    public TMP_Text mainUIEEventRequireItemName; //두가지 이상인경우?

    [Header("subUI 스크롤뷰의 content")]
    public GameObject[] EventListContent;

    private int eventIndex;

    public void SetEventListTitleText(GameEventData data, int _eventIndex)
    {
        CanCompleteBtns.onClick.RemoveAllListeners();
        mainUIEventTitle.text = data.title;
        mainUIEventDesc.text = data.description;
        mainUIEventEffectName.text = data.eventEffectDesc;
        mainUIEEventRequireItemName.text = data.eventRquirementDesc;
        EventClearDetermine(data);
        CanCompleteBtns.onClick.AddListener(() => EventClearOnUI(data));
        eventIndex = _eventIndex;
        Completed.gameObject.SetActive(false); //0704완료됨버튼관련 비활성화조치
    }


    public void SetEventSubUIBtnTitle(GameObject go, int eventIndex) //서브ui타이틀리스트연결용
    {
        CanCompleteBtns.onClick.RemoveAllListeners();
        TMP_Text text = go.GetComponentInChildren<TMP_Text>();
        text.SetText(EventManager.Instance.CurEvents[eventIndex].title);
        //go.GetComponent<Button>().interactable = false; 이벤트의 스테이트에서 완료된경우
            //컬러틴트로 하이라이트와 디스에이블드 색상을 다르게 해줘야함
    }
    public void EventClearDetermine(GameEventData data)
    {
        CanCompleteBtns.gameObject.SetActive(EventManager.Instance.DetermineEventComplete(data));
        CanNotCompleteBtns.gameObject.SetActive(!EventManager.Instance.DetermineEventComplete(data));
    }


    public void EventClearOnUI(GameEventData data)
    {
        CanCompleteBtns.gameObject.SetActive(false);//0704완료가능버튼관련 비활성화조치
        Completed.gameObject.SetActive(true);
        data.isComplete = true;
        EventManager.Instance.EventClear(data, eventIndex);
    }

    
    
}