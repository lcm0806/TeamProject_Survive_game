using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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
        //월드로 씬전환해주고
        SceneSystem.Instance.LoadScene("TEST");
    }

    public void ExitWithNotEnoughOxygenYes()
    {
        //산소를 현재보유량만큼 - 하고
        StatusSystem.SetMinusOxygen(StatusSystem.GetOxygen());
        //월드로 씬을 전환
        SceneSystem.Instance.LoadScene("TEST");
    }

    // 확인창에서 no 눌렀을때 시스템캔버스 자체를 비활성화 시켜서 창을 닫음
    public void DeActivateExitConfirmPanel(int systemCanvas)
    {
        SystemCanvas[systemCanvas].SetActive(false);
    }
}
