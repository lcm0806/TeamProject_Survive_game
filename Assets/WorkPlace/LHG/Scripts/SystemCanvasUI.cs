using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class SystemCanvasUI : MonoBehaviour
{
    [SerializeField] public GameObject ShelterUICanvas;
    public StatusSystem StatusSystem;
    public SceneSystem SceneSystem;
    public TMP_Text[] ExitWithNotEnoughOxygenText;

    
    
    public GameObject[] SystemCanvas; 

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
    private void SleepAndNextDay()
    {
        //오늘밤 이벤트의 효과를 스테이터스에 넘겨주고
        //날짜전환 캔버스를 보여주고
        //날짜를 변경
        //쉘터 기본캔버스를 띄워줌
    }


    

}