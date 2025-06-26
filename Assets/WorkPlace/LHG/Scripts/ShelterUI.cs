using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;


public class ShelterUI : MonoBehaviour
{
    public StatusSystem StatusSystem;

    public GameObject[] ShelterMenu, Tabs, EventContainer, CompleteBtn;
    [SerializeField] public TMP_Text[] Indicators;
    //public Image[] TabButtons;
    //public Sprite InactiveTabBG, ActiveTabBG; // 활성 비활성 시각화를 백그라운드스프라이트로 처리할 때 필요
    [SerializeField] public bool isEventCompleted;
    [SerializeField] public bool canCompleteEvent;
    [SerializeField] private EventState testState; //TODO 이벤트마다 스테이트를 가져야함, 충족시 해당 이벤트의 스테이트를 변경해주도록 만들어야

    private void Start()
    {
        DisplayIndicators(0);
    }
    public void DisplayIndicators(int indicatorsID)
    {
        Indicators[0].SetText($"Current Day :{StatusSystem.GetCurrentDay()}");
        Indicators[1].SetText($"Current Oxygen : {StatusSystem.GetOxygen()}");
    }

    public void ActiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(true);
        Debug.Log($"{ShelterMenuID.ToString()} UI Active");

        EventCompleteBtnSwitcher(testState);
    }
    public void DeactiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(false);
        Debug.Log($"{ShelterMenuID.ToString()} UI DeActive");
    }

    public void ExitUI(GameObject go)
    {
        go.SetActive(false);
        Debug.Log($"{go.name} Exit UI");
    }


    public void SwitchToTab(int TabID)
    {
        EventCompleteBtnSwitcher(testState);

        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
        Tabs[TabID].SetActive(true);

        //foreach (Image im in TabButtons)
        //{
        //    im.sprite = InactiveTabBG;
        //}
        //TabButtons[TabID].sprite = ActiveTabBG;
    }

    
    public void SelectEventList(int EventContainerID)
    {
        foreach (GameObject go in EventContainer)
        { 
            go.SetActive(false); 
        }
        EventContainer[EventContainerID].SetActive(true);
    }

    public void EventCompleteBtnSwitcher(EventState state)
    {
        //버튼엘리먼트 0:완료불가, 1:완료가능, 2:완료됨
        foreach (GameObject go in CompleteBtn)
        {
            go.SetActive(false); //일단 비활성화 초기화 후,
        }

        CompleteBtn[(int)state].SetActive(true); // 배열에담긴버튼과 상태에 따라 액티브
    }

    public void EventCompleteBtn(int CompleteBtnID)
    {
        //TODO 이벤트매니저에서 이벤트완료시 처리되는 함수를 호출 = 종료효과 등

        foreach(GameObject go in CompleteBtn)
        {
            //기존버튼을 비활성화 후
            go.SetActive(false);
            //완료됨 버튼을 활성화
            CompleteBtn[2].SetActive(true);
        }
        
        //TODO 이벤트매니저에 이벤트의 상태를 완료됨으로 변경.
        //TODO 이벤트 리스트 버튼을 회색처리해주고 정렬을 밑으로...
    }

    //이벤트 탭의 서브ui의 이벤트 목록을 정렬해줌
    public void EventListSort()
    {
        //TODO이벤트매니저에서 자료를 가져와서 위에서부터 instantiate해주고, 나중에 추가된거를 제일 위에생성(버티컬레이아웃그룹 앵커나피벗 만지면될듯?)
    }


    //완료,완료불가,완료됨 버튼의 상태 열거형
    public enum EventState
    {
        //상태 변수명 나중에 다시 통일
        CanNotComplete,
        CanComplete,
        AlreadyComplted
    }



}