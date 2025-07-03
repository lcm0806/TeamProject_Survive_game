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

    [Header("subUI 이벤트 리스트의 제목")]
    public TMP_Text[] subUIEventListTitle;

    private int eventIndex;

    public void SetEventListTitleText(GameEventData data, int _eventIndex)
    {
        CanCompleteBtns.onClick.RemoveAllListeners();
        mainUIEventTitle.text = data.title;
        mainUIEventDesc.text = data.description;
        EventClearDetermine(data);
        CanCompleteBtns.onClick.AddListener(() => EventClearOnUI(data));
        eventIndex = _eventIndex;
    }

    public void EventClearDetermine(GameEventData data)
    {
        CanCompleteBtns.gameObject.SetActive(EventManager.Instance.DetermineEventComplete(data));
        CanNotCompleteBtns.gameObject.SetActive(!EventManager.Instance.DetermineEventComplete(data));
    }


    public void EventClearOnUI(GameEventData data)
    {
        Completed.gameObject.SetActive(true);
        data.isComplete = true;
        EventManager.Instance.EventClear(data, eventIndex);
    }

}