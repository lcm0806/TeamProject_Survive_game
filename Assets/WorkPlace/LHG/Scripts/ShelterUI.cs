using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelterUI : MonoBehaviour
{
    public GameObject[] ShelterMenu, Tabs;
    public Image[] TabButtons;
    public Sprite InactiveTabBG, ActiveTabBG; // 탭 활성화 백그라운드 이미지와 비활성화 백그라운드 이미지
    

    

    


    public void ActiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(true);
        Debug.Log($"{ShelterMenuID.ToString()} UI 활성화");
    }
    public void DeactiveUI(int ShelterMenuID)
    {
        ShelterMenu[ShelterMenuID].SetActive(false);
        Debug.Log($"{ShelterMenuID.ToString()} UI 비활성화");
    }

    public void ExitUI(GameObject go)
    {
        go.SetActive(false);
        Debug.Log($"{go.name} 닫음");
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