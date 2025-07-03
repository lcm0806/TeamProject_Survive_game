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

    public void ExitWithNotEnoughOxygenTextDisplay()
    {
        ExitWithNotEnoughOxygenText[0].SetText($"(This choice will use {StatusSystem.GetOxygen()} oxygen to leave the Shelter and go out to explore.\r\nExploration is limited to 'Once' per day.\r\nAre you sure to proceed this decision?");
    }

    public void ExitWithEnoughOxygenYes()
    {
        //산소를 -100하고
        StatusSystem.SetMinusOxygen(-100);

        // Singleton null 체크 추가
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadFarmingScene();
        }
        else
        {
            Debug.LogError("SceneSystem Instance is null!");
        }
    }

    
    public void DeActivateExitConfirmPanel(int systemCanvas)
    {
        SystemCanvas[systemCanvas].SetActive(false);
    }



    public void BedRoomProceedConfirmTextDisplay()
    {
        Debug.Log("잠자기 확인창 텍스트 출력");
        BedRoomProceedConfirmText[0].SetText($"Tonight {"event.name"} will occur. \r\n Once you go to bed, it can't be reversed.\r\n Are you ready to go to sleep after all the preparations?");
    }

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
        // 씬이동 및 저장
        SceneSystem.Instance.LoadSceneWithDelayAndSave(SceneSystem.Instance.GetShelterSceneName());
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
