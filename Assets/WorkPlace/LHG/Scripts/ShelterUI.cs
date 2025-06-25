using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ShelterUI : MonoBehaviour
{
    public GameObject[] ShelterMenu, Tabs;
    public Image[] TabButtons;
    public Sprite InactiveTabBG, ActiveTabBG; // �� Ȱ��ȭ ��׶��� �̹����� ��Ȱ��ȭ ��׶��� �̹���
    
    
    public void ActiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(true);
        Debug.Log($"{ShelterMenuID.ToString()} UI Ȱ��ȭ");
    }
    public void DeactiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(false);
        Debug.Log($"{ShelterMenuID.ToString()} UI ��Ȱ��ȭ");
    }

    public void ExitUI(GameObject go)
    {
        go.SetActive(false);
        Debug.Log($"{go.name} ����");
    }
    

    public void SwitchToTab(int TabID)
    {
        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
        Tabs[TabID].SetActive(true);
        
        
        

        foreach (Image im in TabButtons)
        {
            im.sprite = InactiveTabBG; 
        }
        TabButtons[TabID].sprite = ActiveTabBG;
    }
}