using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject SettingMenu;
    public GameObject ExitMenu;
    public GameObject MainMenu;

    private void Start()
    {
        if (MainMenu == null)
        {
            Debug.LogError("MainMenu가 할당되지 않았습니다!");
            return;
        }
    }

    private void Update()
    {
       MainMenu.transform.Find("StartButton");
    }

    private void OnStartButtonClick()
    {
        Debug.Log("게임 시작!  씬으로 이동합니다.");
    }

    public void ExitMenuBtnClick()
    {
        MainMenu.SetActive(false);
        ExitMenu.SetActive(true);
    }

    public void ExitMenuBtnBack()
    {
        ExitMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void OptionMenuBtnClick()
    {
        MainMenu.SetActive(false);
        SettingMenu.SetActive(true);
    }
    
    public void OptionMenuBtnBack()
    {
        SettingMenu.SetActive(false);
        MainMenu.SetActive(true);
    }
    
}
