using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSystem : MonoBehaviour
{
    [Header("메인 메뉴")] public GameObject MainMenu;
    private Button _mainStartButton;
    private Button _mainContinueButton;
    private Button _mainSetButton;
    private Button _mainCreatorsButton;
    private Button _mainExitButton;

    [Header("설정 메뉴")] public GameObject SettingMenu;
    private Button _settingOkButton;
    private Button _settingBackButton;


    [Header("종료 메뉴")] public GameObject ExitMenu;
    private Button _exitOkButton;
    private Button _exitBackButton;

    private bool _isSaveMode = false;
    private int _selectedSlotIndex = -1;

    private TextMeshProUGUI _menuTitleText;


    private static MenuSystem _instance;
    public static MenuSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MenuSystem>();
                if (_instance == null)
                {
                    var go = new GameObject("MenuSystem");
                    _instance = go.AddComponent<MenuSystem>();
                    DontDestroyOnLoad(go);
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // TODO: TEST
        AudioSystem.Instance.PlayBGMByName("MainBGM");
    }

    private void MainMenuButtons()
    {
        if (MainMenu != null)
        {
            _mainStartButton?.onClick.AddListener(OnMainStartButtonClick);
            _mainSetButton?.onClick.AddListener(OnMainSetButtonClick);
            _mainExitButton?.onClick.AddListener(OnMainExitButtonClick);
            _mainContinueButton?.onClick.AddListener(OnMainContinueButtonClick);
            _mainCreatorsButton?.onClick.AddListener(OnMainCreatorButtonClick);
        }
    }

    private void SettingMenuButtons()
    {
        if (SettingMenu)
        {
            // 뒤로가기 버튼 - 설정 원복
            _settingBackButton?.onClick.AddListener(() =>
            {
                // 오디오 설정 원복
                if (AudioSystem.Instance != null)
                {
                    AudioSystem.Instance.LoadVolumeSettings();
                }

                // 그래픽 설정 원복
                if (GraphicsSystem.Instance != null)
                {
                    GraphicsSystem.Instance.LoadGraphicsSettings();
                }

                SettingMenu.SetActive(false);
                MainMenu.SetActive(true);
            });

            // 확인 버튼 - 설정 저장
            _settingOkButton?.onClick.AddListener(() =>
            {
                // 오디오 설정 저장
                if (AudioSystem.Instance != null)
                {
                    PlayerPrefs.SetFloat(AudioSystem.Instance.BGMVolumeKey, AudioSystem.Instance.BGMVolume);
                    PlayerPrefs.SetFloat(AudioSystem.Instance.SFXVolumeKey, AudioSystem.Instance.SFXVolume);
                }

                // 그래픽 설정 저장
                if (GraphicsSystem.Instance != null)
                {
                    GraphicsSystem.Instance.SaveGraphicsSettings();
                }

                PlayerPrefs.Save();


                SettingMenu.SetActive(false);
                MainMenu.SetActive(true);
            });
        }
    }

    private void ExitMenuButtons()
    {
        if (ExitMenu)
        {
            _exitBackButton?.onClick.AddListener(() =>
            {
                ExitMenu.SetActive(false);
                MainMenu.SetActive(true);
            });
            
            _exitOkButton?.onClick.AddListener(() =>
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });
        }
    }

    /// <summary>
    /// 게임 시작 버튼 누를시
    /// </summary>
    public void OnMainStartButtonClick()
    {
        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.StopBGM();

        // 쉘터 씬 로드
        if (SceneSystem.Instance != null) SceneSystem.Instance.LoadShelterScene();

        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.PlaySFXByName("MainSFX");


        MainMenu.SetActive(false);
    }

    /// <summary>
    /// 메인 메뉴에서 설정 버튼 클릭 시 UI 나오도록
    /// </summary>
    private void OnMainSetButtonClick()
    {
        SettingMenu.SetActive(true);
    }

    /// <summary>
    /// 메인 메뉴에서 종료 버튼 클릭 시 UI 나오도록
    /// </summary>
    private void OnMainExitButtonClick()
    {
        ExitMenu.SetActive(true);
    }
    /// <summary>
    /// 메인 메뉴에서 계속하기 버튼 클릭 시 
    /// </summary>
    private void OnMainContinueButtonClick()
    {
        ExitMenu.SetActive(true);
    }
    
    /// <summary>
    /// 메인 메뉴에서 만든이들 버튼 클릭 시 
    /// </summary>
    private void OnMainCreatorButtonClick()
    {
        ExitMenu.SetActive(true);
    }
}