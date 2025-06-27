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
    private SceneSystem _sceneSystem;
    public TMP_Text[] ExitWithNotEnoughOxygenText;

    public void ExitWithNotEnoughOxygenTextDisplay()
    {
        ExitWithNotEnoughOxygenText[0].SetText($"(This choice will use {StatusSystem.GetOxygen()} oxygen to leave the Shelter and go out to explore.\r\nExploration is limited to 'Once' per day.\r\nAre you sure to proceed this choice?");
    }

    public void ExitWithEnoughOxygenYes()
    {
        //산소를 -100하고
        StatusSystem.SetMinusOxygen(-100);
        //월드로 씬전환해주고
        // _sceneSystem.LoadFarmingScene();

        // 250628 추가
        SceneSystem.Instance.LoadFarmingScene();
    }
}