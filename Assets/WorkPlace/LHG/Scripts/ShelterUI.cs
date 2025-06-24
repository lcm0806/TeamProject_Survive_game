using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShelterUI : MonoBehaviour
{
    public void OnClickMonitor()
    {
        Debug.Log("모니터 클릭됨.");
        //(퀘스트탭 선택된 채로) 메뉴창 팝업
    }

    public void OnClickWorkBench()
    {
        Debug.Log("작업대 클릭됨.");
        //(제작탭 선택된 채로) 메뉴창 팝업
    }

    public void OnClickEntrance()
    {
        Debug.Log("출입구 클릭됨.");
        //(지도탭 선택된 채로) 메뉴창 팝업
    }

    
}
