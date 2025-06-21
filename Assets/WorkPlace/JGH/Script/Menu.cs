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

    private Button _startButton;
    private Button _exitButton;

    private void Start()
    {
        if (MainMenu == null)
        {
            Debug.LogError("MainMenu가 할당되지 않았습니다!");
            return;
        }

        _startButton = MainMenu.transform.Find("StartButton").GetComponent<Button>();
    }

    private void Update()
    {
       _startButton.onClick.AddListener(() =>
       {
            Debug.Log("게임 시작!  씬으로 이동합니다.");
       });

       ExitMenu.transform.Find("Exit").GetComponent<Button>().onClick.AddListener(() =>
       {
           ExitMenu.SetActive(true);
       });
    }

}
