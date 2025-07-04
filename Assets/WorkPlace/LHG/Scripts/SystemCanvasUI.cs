using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class SystemCanvasUI : MonoBehaviour
{
    [SerializeField] public GameObject ShelterUICanvas, LoadingCanvas;
    public StatusSystem StatusSystem;
    public SceneSystem SceneSystem;
    public TMP_Text[] ExitWithNotEnoughOxygenText, NightTransitionText, BedRoomProceedConfirmText;
    public GameObject[] SystemCanvas, LoadingSceneBG;

    public string fullText;
    public float typingSpeed = 0.1f;
    private int currentIndex;

    // 사용안하는 함수 주석처리
    // public void ExitWithEnoughOxygenTextDisplay()
    // {
        // ExitWithEnoughOxygenText[0].SetText("탐색을 시작하겠습니까?\r\n하루에 한번만 탐색이 가능합니다.\n산소를 100 소모합니다.");
        // ExitWithEnoughOxygenText[0].SetText($"탐색을 시작하겠습니까?{System.Environment.NewLine}하루에 한번만 탐색이 가능합니다.{System.Environment.NewLine}산소를 100 소모합니다.");
    // }

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

    // public void BedRoomProceedConfirmTextDisplay()
    // {
    //     Debug.Log("잠자기 확인창 텍스트 출력");
    //     // 수정
    //     BedRoomProceedConfirmText[0].SetText("침대를 이용하시겠습니까");
    // }

    //침실-수면실행시 yes버튼
    public void SleepAndNextDay()
    {
        if (LoadingCanvas.activeSelf == false)
        {
            LoadingCanvas.SetActive(true);
        
            // 코루틴 대신 Invoke 사용
            Invoke(nameof(DelayedSceneTransition), 2f);
        }

        //TODO 게임오버 확인(산소,전력,내구도) - 기훈님께 확인필요(처리 완료)
        // 게임오버
        GameSystem.Instance.CheckGameOver();
    }
    
    private void DelayedSceneTransition()
    {
        // 250702 추가
        // 날짜 + 1
        StatusSystem.Instance.NextCurrentDay();
        // 탐색 여부
        StatusSystem.Instance.SetIsToDay(false);



        // 부정효과 날짜넘어가기직전 시점 *250704 12:30*
        foreach (GameEventData data in EventManager.Instance.CurEvents) //나중에 다시 읽어보기(학습)
        {
            EventManager.Instance.EventEffect(data);
        }
        
        // 씬이동 및 저장
        SceneSystem.Instance.LoadSceneWithDelayAndSave(SceneSystem.Instance.GetShelterSceneName());

        // 새로운 날의 시작(이벤트들을 발생하는 시점) *250704 12:30*
        EventManager.Instance.EventStart();

        LoadingSceneBG[0].SetActive(false);
    }

    //public void NightTransitionTextDisplay()
    //{

    //    StartCoroutine(TypingEffect());
    //    NightTransitionText[0].SetText($"{StatusSystem.GetIsToDay()}일차.. \r\n 이벤트: {"event.name"} 가 발생. \r\n 효과 : {"event.effect"} \r\n ...");
    //}

    //IEnumerator TypingEffect()
    //{
    //    currentIndex = 0;
    //    NightTransitionText[0].text = ""; 
    //    while(currentIndex < fullText.Length)
    //    {
    //        NightTransitionText[0].text += fullText[currentIndex];
    //        currentIndex++;
    //        yield return new WaitForSeconds(typingSpeed);
    //    }
    //}
    


}
