using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class ShelterUI : MonoBehaviour
{
    public StatusSystem StatusSystem;
    public SystemCanvasUI SystemCanvasUI;
    [SerializeField] public GameObject SystemCanvas;


    public GameObject[] ShelterMenu, Tabs, EventContainer, MapLocations, ExitShelterCases;
    [SerializeField] public TMP_Text[] Indicators, TonightEventText;
    public Image[] TabButtons, MapLocationsButtons;
    public Sprite InactiveTabBG, ActiveTabBG; // 활성 비활성 시각화를 백그라운드스프라이트로 처리할 때 필요


    //0704 실시간으로 인디케이터에 수치를 반영하기 위한 변수
    private double prevOxygen;
    private double prevEnergy;
    private double prevDurability;



    public GameObject Renpy;
    
    private void Start()
    {
        AudioSystem.Instance.StopBGM();
        AudioSystem.Instance.PlayBGMByName("In the Science Lab");
        DisplayIndicators(0);

        // 초기 값 캐싱
        prevOxygen = StatusSystem.Instance.GetOxygen();
        prevEnergy = StatusSystem.Instance.GetEnergy();
        prevDurability = StatusSystem.Instance.GetDurability();
    }

    private void Update()
    {
        double curOxygen = StatusSystem.Instance.GetOxygen();
        double curEnergy = StatusSystem.Instance.GetEnergy();
        double curDurability = StatusSystem.Instance.GetDurability();

        // 값이 변경되었을 때만 UI 갱신
        if (!Mathf.Approximately((float)prevOxygen, (float)curOxygen) ||
            !Mathf.Approximately((float)prevEnergy, (float)curEnergy) ||
            !Mathf.Approximately((float)prevDurability, (float)curDurability))
        {
            DisplayIndicators(0);
            prevOxygen = curOxygen;
            prevEnergy = curEnergy;
            prevDurability = curDurability;
        }
    }

    public void DisplayIndicators(int indicatorsID)
    {
        //[패널상단] 0:날짜 1:산소 / [패널]-[맵]-[쉘터]-[subUI] 2:산소, 3:전력(Energy/Electricity 용어통일필요?), 4:내구도
        Indicators[0].SetText($"Day : {StatusSystem.Instance.GetCurrentDay()}");
        Indicators[1].SetText($"Oxygen : {StatusSystem.Instance.GetOxygen()}");
        Indicators[2].SetText($"Oxygen : {StatusSystem.Instance.GetOxygen()}");
        Indicators[3].SetText($"Energy : {StatusSystem.Instance.GetEnergy()}");
        Indicators[4].SetText($"Durability : {StatusSystem.Instance.GetDurability()}");
        Indicators[5].SetText($"Oxygen : {StatusSystem.Instance.GetOxygen()}");
        Indicators[6].SetText($"Energy : {StatusSystem.Instance.GetEnergy()}");
        Indicators[7].SetText($"Durability : {StatusSystem.Instance.GetDurability()}");
    }

    public void ActiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(true);
        Debug.Log($"{ShelterMenuID} UI Active");

        switch (ShelterMenuID)
        {
            case 0: // 모니터 → 이벤트 탭
                SwitchToTab(0); // 탭 전환
                AutoSelectFirstEvent(); // 제일 위 이벤트 자동 선택
                break;
            case 1: // 워크벤치 → 제작 탭
                SwitchToTab(2); // 탭 전환
                                // TODO: 제작 탭 초기화 코드 추가 예정
                break;
            case 2: // 맵 → 맵 탭 + 쉘터 자동 선택
                SwitchToTab(1);
                SelectMapList(1); // 쉘터 선택 (0:침실, 1:쉘터, 2:출구)
                break;
        }
    }

    private void AutoSelectFirstEvent()
    {
        if (EventManager.Instance.CurEvents.Count > 0)
        {
            var firstEvent = EventManager.Instance.CurEvents[0];
            var eventUI = FindObjectOfType<EventUI>(); // 참조 방식에 따라 수정
            eventUI.SetEventListTitleText(firstEvent, 0); // 첫 번째 이벤트 출력
        }
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
        //Tabs [패널탭] 0:이벤트, 1:맵, 2:제작
        //TabButtons(이미지변경용엘리먼트) [패널탭] 0:이벤트, 1:맵, 2:제작 

        //EventCompleteBtnSwitcher(testState);

        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
        Tabs[TabID].SetActive(true);

        foreach (Image im in TabButtons)
        {
            im.sprite = InactiveTabBG;
        }
        TabButtons[TabID].sprite = ActiveTabBG;
    }


    //패널-이벤트-서브ui이벤트리스트를 누르면 해당 메인ui가 액티브
    public void SelectEventList(int EventContainerID)
    {
        foreach (GameObject go in EventContainer)
        { 
            go.SetActive(false); 
        }
        EventContainer[EventContainerID].SetActive(true);
    }


    //패널-맵- 침실,쉘터,출구를 선택하면 해당 서브ui가 액티브
    public void SelectMapList(int MapLocationsID)
    {
        //MapLocations 0:침실, 1:쉘터, 2,출구
        foreach (GameObject go in MapLocations)
        {
            go.SetActive(false);
        }
        MapLocations[MapLocationsID].SetActive(true);

        //[패널-지도탭] 0:침실 1: 쉘터 2:출구
        foreach (Image im in MapLocationsButtons)
        {
            im.sprite = InactiveTabBG;
        }
        MapLocationsButtons[MapLocationsID].sprite = ActiveTabBG;

    }



    public void ExitShelter()
    {
        //시스템캔버스를 active해줘야함
        Debug.Log("출구 버튼눌림");
        if (SystemCanvas.activeSelf == false) //activeSelf이부분살펴보기
        {
            SystemCanvas.SetActive(true);
            Debug.Log("시스템캔버스 액티브");

            //ID 0:100이넘는경우ui창, 1:100미만인경우ui창, 2:오늘이미탐색한경우ui창
            //오늘 탐색여부를 확인
            //false라면
            if (StatusSystem.Instance.GetIsToDay() == false)
            {

                Debug.Log("오늘 탐색가능");
                // 산소보유량이 100이 넘는 경우
                if (StatusSystem.Instance.GetOxygen() >= 100)
                {

                    Debug.Log("보유산소 100이상");
                    foreach (GameObject go in ExitShelterCases)
                    {
                        Debug.Log("기존ui비활성");
                        go.SetActive(false);
                    }

                    ExitShelterCases[0].SetActive(true);
                    //확인ui을 띄움
                    //실행클릭시 산소를 100소모하고 씬전환
                    //취소클릭시 창닫기

                }
                // 산소보유량이 100미만인 경우(=산소부족시)
                else
                {
                    Debug.Log("보유산소 100미만");
                    SystemCanvasUI.ExitWithEnoughOxygenTextDisplay();
                    foreach (GameObject go in ExitShelterCases)
                    {
                        go.SetActive(false);
                    }

                    ExitShelterCases[1].SetActive(true);
                    //확인ui을 띄우고
                    //실행클릭시 남은 모든 산소를 소모하고 씬전환
                    //취소클릭시 창닫기

                }
                //만약 산소가 0인..경우?
            }
            // 오늘 이미 탐색했다면 
            else
            {
                Debug.Log("오늘 이미 탐색");
                foreach (GameObject go in ExitShelterCases)
                {
                    go.SetActive(false);
                }

                ExitShelterCases[2].SetActive(true);
            }
        }
    }


    //public void EventCompleteBtnSwitcher(EventState state)
    //{
    //    //버튼엘리먼트 0:완료불가, 1:완료가능, 2:완료됨
    //    foreach (GameObject go in CompleteBtn)
    //    {
    //        go.SetActive(false); //일단 비활성화 초기화 후,
    //    }

    //    CompleteBtn[(int)state].SetActive(true); // 배열에담긴버튼과 상태에 따라 액티브
    //}

    //public void EventCompleteBtn(int CompleteBtnID)
    //{
    //    //TODO 이벤트매니저에서 이벤트완료시 처리되는 함수를 호출 = 종료효과 등

    //    foreach(GameObject go in CompleteBtn)
    //    {
    //        //기존버튼을 비활성화 후
    //        go.SetActive(false);
    //        //완료됨 버튼을 활성화
    //        CompleteBtn[2].SetActive(true);
    //    }
        
    //    //TODO 이벤트매니저에 이벤트의 상태를 완료됨으로 변경.
    //    //TODO 이벤트 리스트 버튼을 회색처리해주고 정렬을 밑으로...
    //}

    ////완료,완료불가,완료됨 버튼의 상태 열거형
    //public enum EventState
    //{
    //    //상태 변수명 나중에 다시 통일
    //    CanNotComplete,
    //    CanComplete,
    //    AlreadyComplted
    //}

    private void TonightEventDisplay()
    {
        //오늘밤의 이벤트 이름과 효과를 보여주고
        //TODO 이벤트 시스템에서 값을 가져와야 함
        //TODO 복수의 이벤트이름과 효과가 있는 경우..어떻게? 반복문?
        TonightEventText[0].SetText($"{"이벤트이름"} : {"이벤트효과"}");
    }



    //침실 잠자기 버튼
    public void GoToSleep()
    {
        Debug.Log("잠자기 버튼눌림");
        if (SystemCanvas.activeSelf == false) 
        {
            SystemCanvas.SetActive(true);
            Debug.Log("시스템캔버스 액티브");

            //잠잘때는 경우의수가 없음
            //exit shelter cases배열의 4번에 그냥 배치하고 불러오자

            foreach (GameObject go in ExitShelterCases)
            {
                Debug.Log("기존ui비활성");
                go.SetActive(false);
            }
            ExitShelterCases[3].SetActive(true);
            // SystemCanvasUI.BedRoomProceedConfirmTextDisplay();
        }
    }
}