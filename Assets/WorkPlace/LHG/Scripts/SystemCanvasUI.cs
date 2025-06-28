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
    public TMP_Text[] ExitWithNotEnoughOxygenText, DayTransitionText;

    
    
    public GameObject[] SystemCanvas, LoadingSceneBG; 

    public void ExitWithNotEnoughOxygenTextDisplay()
    {
        ExitWithNotEnoughOxygenText[0].SetText($"(This choice will use {StatusSystem.GetOxygen()} oxygen to leave the Shelter and go out to explore.\r\nExploration is limited to 'Once' per day.\r\nAre you sure to proceed this choice?");
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

    // Ȯ��â���� no �������� �ý���ĵ���� ��ü�� ��Ȱ��ȭ ���Ѽ� â�� ����
    public void DeActivateExitConfirmPanel(int systemCanvas)
    {
        SystemCanvas[systemCanvas].SetActive(false);
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
    }
    
    private void DelayedSceneTransition()
    {
        SceneSystem.Instance.LoadDayTransitionScene();
        LoadingSceneBG[0].SetActive(false);
    }
    
}
