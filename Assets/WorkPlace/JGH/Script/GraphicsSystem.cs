using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using DesignPattern;

[System.Serializable]
public class GraphicsSettings
{
    public bool fullScreen = true;
    public int qualityLevel = 2;
    public bool vsync = true;
    public int targetFrameRate = 60;
}

public class GraphicsSystem : Singleton<GraphicsSystem>
{
    [Header("설정 메뉴 UI 요소들")]
    [SerializeField] private Button _fullscreenCheckbox;
    [SerializeField] private TextMeshProUGUI _fullscreenText;
    [SerializeField] private Button _qualityButton;
    [SerializeField] private TextMeshProUGUI _qualityLabel;
    
    
    // 설정 변경 시 자동 저장을 위한 변수
    private bool _autoSaveEnabled = true;
    private Coroutine _autoSaveCoroutine;
    
    
    [Header("버튼들")]
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    
    [Header("그래픽 설정")]
    [SerializeField] private GraphicsSettings _graphicsSettings = new GraphicsSettings();
    
    private bool _isFullscreen = true;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 해상도 지정
        Screen.SetResolution(1920, 1080, true);
    }
        
}