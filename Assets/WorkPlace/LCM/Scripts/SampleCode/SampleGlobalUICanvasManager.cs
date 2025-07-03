using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPattern;

public class SampleGlobalUICanvasManager : Singleton<SampleGlobalUICanvasManager>
{
    private void Awake()
    {
        // 상위 클래스인 Singleton<GlobalUICanvasManager>의 초기화 메서드를 호출합니다.
        // 이 안에서 DontDestroyOnLoad(this.gameObject)가 처리됩니다.
        SingletonInit();
    }
}
