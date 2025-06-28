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

        //����� ����ȯ���ְ�
        SceneSystem.Instance.LoadFarmingScene();
    }

    public void ExitWithNotEnoughOxygenYes()
    {
        //��Ҹ� ���纸������ŭ - �ϰ�
        StatusSystem.SetMinusOxygen(StatusSystem.GetOxygen());
        //����� ���� ��ȯ
        SceneSystem.Instance.LoadFarmingScene();
    }

    // Ȯ��â���� no �������� �ý���ĵ���� ��ü�� ��Ȱ��ȭ ���Ѽ� â�� ����
    public void DeActivateExitConfirmPanel(int systemCanvas)
    {
        SystemCanvas[systemCanvas].SetActive(false);
    }


    
    


    //침실-수면실행시 yes버튼
    public void SleepAndNextDay()
    {
        //TODO 오늘밤 이벤트의 효과를 스테이터스시스템에 넘겨주고

        //TODO 날짜전환 캔버스를 보여주고


        if (LoadingCanvas.activeSelf == false)
        {
            

            StartCoroutine(DelayTime());

            IEnumerator DelayTime()
            {
                LoadingCanvas.SetActive(true);

                yield return new WaitForSeconds(1f);
            }


            //날짜및 스탯을 변경
            SceneSystem.Instance.LoadDayTransitionScene(); //다음날로 스탯을 넘겨줌
            SceneSystem.Instance.LoadShelterScene(); //다시쉘터씬처음띄워줌 자기전세이브와, 일어난후 세이브도 포함되어있음
            LoadingSceneBG[0].SetActive(false);

        }
        
    }




}