using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class SystemCanvasUI : MonoBehaviour
{
    [SerializeField] private DayTransitionUI dayTransitionUI;
    [SerializeField] public GameObject ShelterUICanvas, LoadingCanvas;
    public StatusSystem StatusSystem;
    public SceneSystem SceneSystem;
    public TMP_Text[] ExitWithNotEnoughOxygenText, NightTransitionText, BedRoomProceedConfirmText;
    public GameObject[] SystemCanvas, LoadingSceneBG;

    public string fullText;
    public float typingSpeed = 0.1f;
    private int currentIndex;

    // 사용안하는 함수 주석처리
    public void ExitWithEnoughOxygenTextDisplay()
    {
        ExitWithNotEnoughOxygenText[0].SetText($"산소를 {StatusSystem.Instance.GetOxygen()} 소모해 탐색에 나갑니다.\r\n탐색은 하루에 한번만 가능합니다.\r\n정말로 나가시겠습니까?");
    }
    
    public void ExitNotWithEnoughOxygenYes()
    {
        //산소를 현재 가지고있는 산소 만큼 제거
        StatusSystem.Instance.SetMinusOxygen(StatusSystem.Instance.GetOxygen());
        
        // 탐색한걸로 세팅 
        StatusSystem.Instance.SetIsToDay(true);
        
        // Singleton null 체크 추가
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetFarmingSceneName());
        }
        else
        {
            Debug.LogError("SceneSystem Instance is null!");
        }
    }

    public void ExitWithEnoughOxygenYes()
    {
        //산소를 -100하고
        StatusSystem.Instance.SetMinusOxygen(100);
        
        // 탐색한걸로 세팅 
        StatusSystem.Instance.SetIsToDay(true);
        
        // Singleton null 체크 추가
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetFarmingSceneName());
        }
        else
        {
            Debug.LogError("SceneSystem Instance is null!");
        }
    }


    public void DeActivateExitConfirmPanel(int systemCanvas)
    {
        // 250703 배열 값 벗어나서 오류남
        // SystemCanvas[systemCanvas].SetActive(false);
        SystemCanvas[SystemCanvas.Length - 1].SetActive(false);

    }



    //침실-수면실행시 yes버튼
    public void SleepAndNextDay()
    {
        if (LoadingCanvas.activeSelf == false)
        {
            LoadingCanvas.SetActive(true);
            var uncompleted = EventManager.Instance.GetUnCompletedEvents(); // 따로 만들어야 함
            dayTransitionUI.StartDayTransition(uncompleted);
        }

    }


        
    //    // 탐색 여부
    //    StatusSystem.Instance.SetIsToDay(false);



        
        
    //    // 씬이동 및 저장
    //    SceneSystem.Instance.LoadSceneWithDelayAndSave(SceneSystem.Instance.GetShelterSceneName());

    //    // 새로운 날의 시작(이벤트들을 발생하는 시점) *250704 12:30*
    //    EventManager.Instance.EventStart();

    //    LoadingSceneBG[0].SetActive(false);
    //}
}
