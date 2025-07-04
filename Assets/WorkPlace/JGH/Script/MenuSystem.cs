using System;
using System.Collections;
using UnityEngine;
using DesignPattern;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Button = UnityEngine.UI.Button;
using Cursor = UnityEngine.Cursor;
using Input = UnityEngine.Input;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;


[Serializable]
public class SettingData
{
    public bool fullscreen;
    public int quality;
    public float bgmVolume;
    public float sfxVolume;
}

public class MenuSystem : Singleton<MenuSystem>
{
    [Header("메인 메뉴")] 
    public GameObject MainMenu;
    private Button _mainStartButton;
    private Button _mainContinueButton;
    private Button _mainSetButton;
    private Button _mainCreatorsButton;
    private Button _mainExitButton;
    
    [Header("설정 메뉴")] 
    public GameObject SettingMenu;
    private Toggle _fullscreenCheckBox;       // CheckBox 버튼
    private Button _fullscreenYesButton;      // YesButton
    private Button _fullscreenNoButton;       // NoButton
    private Button _fullscreenQualityButton;            // QualityButton 버튼
    private Slider _bgmSlider;             // BGM_Slider (사용자가 Scrollbar로 변경)
    private Slider _sfxSlider;             // SFX_Slider (사용자가 Scrollbar로 변경)
    private TMP_Text _bgmValueText;               // BGM_PEC
    private TMP_Text _sfxValueText;               // SFX_PEC
    private TMP_Text _qualityText; 

    [Header("일시정지 메뉴")] 
    public GameObject PauseMenu;
    private Button _pauseSetButton;
    private Button _pauseBackToMenuButton;
    
    [Header("종료 메뉴")] 
    public GameObject ExitMenu;
    private Button _exitOkButton;
    private Button _exitBackButton;

    [Header("확인 메뉴")]
    public GameObject NewGameDialog;
    private Button _newGameOkButton;
    private Button _newGameBackButton;
    
    public GameObject ContinueDialog;
    private Button _continueDialogOkButton;
    private Button _continueDialogBackButton;
    
    public GameObject CreatorsDialog;
    private Button _creatorsYesButton;
    
    public GameObject GameOverDialog;
    private Button _gameOverYesButton;
    
    [Header("경고 메뉴")]
    public GameObject WarningDialog;
    private Button _warningYesButton;

    public GameObject BackToMenuDialog;
    private Button _backToMenuDialogNoButton;
    private Button _backToMenuDialogYesButton;
    
    
    // 전체화면 상태 관리
    private bool _isFullscreenEnabled = false;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private GameObject _checkmarkImage;
    
    // 메뉴 상태 관리
    private GameObject _currentOpenMenu = null;
    private bool _isMenuOpen = false;
    
    // 설정 업데이트 플래그
    private bool _isUpdatingSettings = false;
    
    // 이전 메뉴 추적
    private GameObject _previousMenu = null;
    
    // 현재 어떤 설정인지 추적
    private bool _isPauseSettingMode = false;
    
    // 설정값 백업
    private bool _tempFullscreen;
    private int _tempQuality;
    private float _tempBGM;
    private float _tempSFX;

    // 현재 설정값
    private bool _currentFullscreen;
    private int _currentQuality;
    private float _currentBGM;
    private float _currentSFX;
    

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        AudioSystem.Instance.StopBGM();
        AudioSystem.Instance.PlayBGMByName("SwingJazzMarsMellow");
        
        InitializeMainMenu();
        InitializeSettingMenu();
        InitializeOtherMenus();
        LoadAndApplySettings();
    }

    void Update()
    {
        // ESC 키 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TitleScene에서는 무시
            if (SceneManager.GetActiveScene().name == SceneSystem.Instance.GetTitleSceneName())
            {
                return;
            }
            
            TogglePauseMenu();
        }
    }
    
    private void TogglePauseMenu()
    {
        bool isCurrentlyActive = PauseMenu.activeSelf;

        AllMenuFalse(); // 항상 모든 메뉴 비활성화
        

        if (!isCurrentlyActive)
        {
            // PauseMenu를 열 때
            PauseMenu.SetActive(true);
            Time.timeScale = 0f;
            
            if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true; 
            }
            
        }
        else
        {
            // PauseMenu를 닫을 때
            PauseMenu.SetActive(false);
            Time.timeScale = 1f;
            
            if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true; 
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true; 
            }

        }
    }

    
    private void InitializeMainMenu()
    {
        if (MainMenu != null)
        {
            _mainStartButton = MainMenu.transform.Find("StartButton").GetComponent<Button>();
            _mainContinueButton = MainMenu.transform.Find("ContinueButton").GetComponent<Button>();
            _mainSetButton = MainMenu.transform.Find("SettingButton").GetComponent<Button>();
            _mainCreatorsButton = MainMenu.transform.Find("CreatorsButton").GetComponent<Button>();
            _mainExitButton = MainMenu.transform.Find("ExitButton").GetComponent<Button>();

            _mainStartButton.onClick.AddListener(OnClickStart);
            _mainContinueButton.onClick.AddListener(OnClickContinue);
            _mainSetButton.onClick.AddListener(OnClickSetting);
            _mainCreatorsButton.onClick.AddListener(OnClickCreators);
            _mainExitButton.onClick.AddListener(OnClickExit);
        }
    }

    private void InitializeSettingMenu()
    {
        if (SettingMenu != null)
        {
            // 컴포넌트 찾기
            _fullscreenCheckBox = SettingMenu.transform.Find("CheckBox").GetComponent<Toggle>();
            _fullscreenYesButton = SettingMenu.transform.Find("YesButton").GetComponent<Button>();
            _fullscreenNoButton = SettingMenu.transform.Find("NoButton").GetComponent<Button>();
            _fullscreenQualityButton = SettingMenu.transform.Find("QualityButton").GetComponent<Button>();

            // 슬라이더 찾기 및 설정
            _bgmSlider = SettingMenu.transform.Find("BGM_Slider").GetComponent<Slider>();
            _sfxSlider = SettingMenu.transform.Find("SFX_Slider").GetComponent<Slider>();

            _bgmValueText = SettingMenu.transform.Find("BGM_PEC").GetComponent<TMP_Text>();
            _sfxValueText = SettingMenu.transform.Find("SFX_PEC").GetComponent<TMP_Text>();

            // 슬라이더 설정
            SetupSlider(_bgmSlider, "BGM");
            SetupSlider(_sfxSlider, "SFX");

            // 이벤트 등록
            if (_bgmSlider != null)
            {
                _bgmSlider.onValueChanged.RemoveAllListeners();
                _bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.RemoveAllListeners();
                _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            }

            // 다른 버튼 이벤트 등록
            _fullscreenCheckBox.onValueChanged.AddListener(ApplyCheckBox);
            _fullscreenYesButton.onClick.AddListener(FullscreenYes);
            _fullscreenNoButton.onClick.AddListener(FullscreenNo);
            _fullscreenQualityButton.onClick.AddListener(ChangeQuality);
        }
    }

    void SetupSlider(Slider slider, string sliderName)
    {
        if (slider == null)
        {
            Debug.LogError($"{sliderName} Slider를 찾을 수 없습니다!");
            return;
        }

        // 슬라이더 기본 설정
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = true;
    
        // Handle Rect 설정 및 크기 조정
        if (slider.handleRect == null)
        {
            Transform handleTransform = slider.transform.Find("Handle Slide Area/Handle");
            if (handleTransform != null)
            {
                slider.handleRect = handleTransform.GetComponent<RectTransform>();
            }
        }

        // Handle 크기 정상화
        if (slider.handleRect != null)
        {
            RectTransform handleRect = slider.handleRect;
            handleRect.localScale = Vector3.one; // 스케일 초기화
            handleRect.sizeDelta = new Vector2(50f, 5f); // Handle 크기 설정
        }

        // Fill Rect 설정
        if (slider.fillRect == null)
        {
            Transform fillTransform = slider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
            {
                slider.fillRect = fillTransform.GetComponent<RectTransform>();
            }
        }

        Debug.Log($"{sliderName} Slider 설정 완료");
    }

    private void InitializeOtherMenus()
    {
        if (PauseMenu != null)
        {
            _pauseSetButton = PauseMenu.transform.Find("SetButton").GetComponent<Button>();
            _pauseBackToMenuButton = PauseMenu.transform.Find("BackToMenu").GetComponent<Button>();
            _pauseSetButton.onClick.AddListener(OnClickPauseSetting);
            _pauseBackToMenuButton.onClick.AddListener(OnClickPauseExit);
        }

        if (ExitMenu != null)
        {
            _exitOkButton = ExitMenu.transform.Find("OkButton").GetComponent<Button>();
            _exitBackButton = ExitMenu.transform.Find("BackButton").GetComponent<Button>();
            _exitOkButton.onClick.AddListener(OnClickExitOk);
            _exitBackButton.onClick.AddListener(OnClickExitBack);
        }

        if (NewGameDialog != null)
        {
            _newGameOkButton = NewGameDialog.transform.Find("OkButton").GetComponent<Button>();
            _newGameBackButton = NewGameDialog.transform.Find("BackButton").GetComponent<Button>();
            _newGameOkButton.onClick.AddListener(OnClickNewGameOk);
            _newGameBackButton.onClick.AddListener(OnClickNewGameBack);
        }

        if (ContinueDialog != null)
        {
            _continueDialogOkButton = ContinueDialog.transform.Find("OkButton").GetComponent<Button>();
            _continueDialogBackButton = ContinueDialog.transform.Find("BackButton").GetComponent<Button>();
            _continueDialogOkButton.onClick.AddListener(OnClickContinueOk);
            _continueDialogBackButton.onClick.AddListener(OnClickContinueBack);
        }

        if (CreatorsDialog != null)
        {
            _creatorsYesButton = CreatorsDialog.transform.Find("YesButton").GetComponent<Button>();
            _creatorsYesButton.onClick.AddListener(OnClickCreatorsYes);
        }


        if (WarningDialog != null)
        {
            _warningYesButton = WarningDialog.transform.Find("YesButton").GetComponent<Button>();
            _warningYesButton.onClick.AddListener(OnClickWarningYes);
        }

        if (BackToMenuDialog != null)
        {
            _backToMenuDialogYesButton = BackToMenuDialog.transform.Find("YesButton").GetComponent<Button>();
            _backToMenuDialogNoButton = BackToMenuDialog.transform.Find("NoButton").GetComponent<Button>();
            _backToMenuDialogYesButton.onClick.AddListener(OnClickBackToMenuYes);
            _backToMenuDialogNoButton.onClick.AddListener(OnClickBackToMenuNo);
        }

        if (GameOverDialog != null)
        {
            _gameOverYesButton= GameOverDialog.transform.Find("YesButton").GetComponent<Button>();
            _gameOverYesButton.onClick.AddListener(OnClickGameOverYes);
        }
    }

    private void LoadAndApplySettings()
    {
        var setting = FileSystem.Instance.LoadSetting();

        _currentFullscreen = setting.fullscreen;
        _currentQuality = setting.quality;
        _currentBGM = setting.bgmVolume;
        _currentSFX = setting.sfxVolume;

        // 백업용에도 복사
        _tempFullscreen = _currentFullscreen;
        _tempQuality = _currentQuality;
        _tempBGM = _currentBGM;
        _tempSFX = _currentSFX;

        // AudioSystem에 현재 볼륨 설정
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.BGMVolume = _currentBGM;
            AudioSystem.Instance.SFXVolume = _currentSFX;
            AudioSystem.Instance.OnBGMVolumeChanged(_currentBGM);
            AudioSystem.Instance.OnSFXVolumeChanged(_currentSFX);
        }

        // UI 반영 (이벤트 발생 방지)
        _isUpdatingSettings = true;
        
        if (_fullscreenCheckBox != null)
            _fullscreenCheckBox.isOn = _currentFullscreen;
            
        if (_bgmSlider != null)
        {
            _bgmSlider.value = _currentBGM;
            Debug.Log($"BGM Slider value set to: {_currentBGM}");
        }
        
        if (_sfxSlider != null)
        {
            _sfxSlider.value = _currentSFX;
            Debug.Log($"SFX Slider value set to: {_currentSFX}");
        }
        
        _isUpdatingSettings = false;

        // 텍스트 업데이트
        UpdateVolumeTexts();

        // 실제 설정 적용
        ApplyCheckBox(_currentFullscreen);
        QualitySettings.SetQualityLevel(_currentQuality);
        
        if (SettingMenu != null)
        {
            _qualityText = SettingMenu.transform.Find("QualityButton").Find("Label").GetComponent<TMP_Text>();
            if (_qualityText != null)
                _qualityText.text = QualitySettings.names[_tempQuality];
        }
    }

    // BGM 슬라이더 변경 시 호출
    private void OnBGMSliderChanged(float value)
    {
        if (_isUpdatingSettings) return;
        
        Debug.Log($"BGM Slider changed to: {value}");
        
        _tempBGM = value;
        
        // AudioSystem을 통해 볼륨 적용
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.OnBGMVolumeChanged(value);
        }
        
        UpdateVolumeTexts();
    }

    // SFX 슬라이더 변경 시 호출
    private void OnSFXSliderChanged(float value)
    {
        if (_isUpdatingSettings) return;
        
        Debug.Log($"SFX Slider changed to: {value}");
        
        _tempSFX = value;
        
        // AudioSystem을 통해 볼륨 적용
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.OnSFXVolumeChanged(value);
        }
        
        UpdateVolumeTexts();
    }

    private void UpdateVolumeTexts()
    {
        if (_bgmValueText != null)
            _bgmValueText.text = Mathf.RoundToInt(_tempBGM * 100) + "%";
        if (_sfxValueText != null)
            _sfxValueText.text = Mathf.RoundToInt(_tempSFX * 100) + "%";
    }
    
    private void ApplyCheckBox(bool isOn) 
    {
        _tempFullscreen = isOn;
        _isFullscreenEnabled = isOn;
        Screen.fullScreen = isOn;
        
        if (_checkmarkImage != null)
        {
            _checkmarkImage.SetActive(isOn);
        }
    }

    private void FullscreenYes() 
    {
        // 실제 저장값 갱신
        _currentFullscreen = _tempFullscreen;
        _currentQuality = _tempQuality;
        _currentBGM = _tempBGM;
        _currentSFX = _tempSFX;

        // AudioSystem에 최종 볼륨 적용
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.BGMVolume = _currentBGM;
            AudioSystem.Instance.SFXVolume = _currentSFX;
            AudioSystem.Instance.OnBGMVolumeChanged(_currentBGM);
            AudioSystem.Instance.OnSFXVolumeChanged(_currentSFX);
        }

        // 저장
        FileSystem.Instance.SaveSetting(new SettingData {
            fullscreen = _currentFullscreen,
            quality = _currentQuality,
            bgmVolume = _currentBGM,
            sfxVolume = _currentSFX
        });
        
        if (_isPauseSettingMode == true)
        {
            SettingMenu.SetActive(false);
            PauseMenu.SetActive(true);
        }
        else
        {
            SettingMenu.SetActive(false);
        }
    }

    private void FullscreenNo()
    {
        // 임시로 바꾼 설정 원복
        _tempFullscreen = _currentFullscreen;
        _tempQuality = _currentQuality;
        _tempBGM = _currentBGM;
        _tempSFX = _currentSFX;

        // AudioSystem에 원래 볼륨 복원
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.BGMVolume = _currentBGM;
            AudioSystem.Instance.SFXVolume = _currentSFX;
            AudioSystem.Instance.OnBGMVolumeChanged(_currentBGM);
            AudioSystem.Instance.OnSFXVolumeChanged(_currentSFX);
        }

        // UI 복원
        _isUpdatingSettings = true;
        
        if (_fullscreenCheckBox != null)
            _fullscreenCheckBox.isOn = _currentFullscreen;
        if (_bgmSlider != null)
            _bgmSlider.value = _currentBGM;
        if (_sfxSlider != null)
            _sfxSlider.value = _currentSFX;
            
        _isUpdatingSettings = false;

        UpdateVolumeTexts();

        // 실제 적용
        Screen.fullScreen = _currentFullscreen;
        QualitySettings.SetQualityLevel(_currentQuality);
        
        if (_isPauseSettingMode == true)
        {
            SettingMenu.SetActive(false);
            PauseMenu.SetActive(true);
        }
        else
        {
            SettingMenu.SetActive(false);
        }
    }

    private void ChangeQuality()
    {
        _tempQuality = (_tempQuality + 1) % QualitySettings.names.Length;
        QualitySettings.SetQualityLevel(_tempQuality);
        
        if (_qualityText != null)
            _qualityText.text = QualitySettings.names[_tempQuality];
        
        Debug.Log("퀄리티 변경: " + _tempQuality);
    }

    // 나머지 메뉴 관련 메서드들...
    private void OnClickStart()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        NewGameDialog.SetActive(true);
    }

    private void OnClickContinue()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        ContinueDialog.SetActive(true);
    }

    private void OnClickSetting()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        _isPauseSettingMode = false;
        SettingMenu.SetActive(true);
    }

    private void OnClickCreators()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        CreatorsDialog.SetActive(true);
    }

    private void OnClickExit()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        ExitMenu.SetActive(true);
    }

    private void OnClickPauseSetting()
    {
        AllMenuFalse();
        MainMenu.SetActive(false);
        _isPauseSettingMode = true;
        SettingMenu.SetActive(true);
        
    }

    private void OnClickPauseExit()
    {
        AllMenuFalse();
        MainMenu.SetActive(false);
        BackToMenuDialog.SetActive(true);
    }

    private void OnClickExitOk()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnClickExitBack()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
        ExitMenu.SetActive(false);
    }

    private void AllMenuFalse()
    {
        NewGameDialog.SetActive(false);
        ExitMenu.SetActive(false);
        ContinueDialog.SetActive(false);
        CreatorsDialog.SetActive(false);
        WarningDialog.SetActive(false);
        PauseMenu.SetActive(false);
        SettingMenu.SetActive(false);
        BackToMenuDialog.SetActive(false);
    }

    private void OnClickNewGameOk()
    {
        if (StatusSystem.Instance != null)
        {
            StatusSystem.Instance.SetCurrentDay(1);
            // StatusSystem.Instance.SetOxygen(100f);
            StatusSystem.Instance.SetOxygen(90f);
            StatusSystem.Instance.SetEnergy(300f);
            StatusSystem.Instance.SetDurability(100f);
            StatusSystem.Instance.SetIsToDay(false);
            Debug.Log("StatusSystem 기본값으로 초기화 완료");
        }

        FileSystem.Instance.DeleteGameSaveData();
        SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetPlologSceneName());
        
        MainMenu.SetActive(false);
        
        AllMenuFalse();
    }
    private void OnClickNewGameBack()
    {
        AllMenuFalse();
        NewGameDialog.SetActive(false);
    }

    private void OnClickContinueOk()
    {
        if (!FileSystem.Instance.HasSaveData())
        {
            ContinueDialog.SetActive(false);
            WarningDialog.SetActive(true);
        }
        
        // gamedata.json 불러오기
        GameData data = FileSystem.Instance.LoadGameData();

        if (data != null)
        {
            // StatusSystem에 값 적용
            FileSystem.Instance.ApplyGameData(data);
            Debug.Log("게임 데이터 불러오기 및 적용 완료");

            // 이어하기 씬 전환
            SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetShelterSceneName());
            
            ContinueDialog.SetActive(false);
            AllMenuFalse();
        }
        else
        {
            Debug.LogWarning("게임 데이터가 존재하지 않음. 기본값 또는 경고 처리 필요");
            ContinueDialog.SetActive(false); // 이어하기 팝업 닫기
            WarningDialog.SetActive(true); // 이어하기 팝업 닫기
        }

    }
    
    private void OnClickContinueBack()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
    }

    private void OnClickCreatorsYes()
    {
        AllMenuFalse();
        MainMenu.SetActive(true);
    }

    private void OnClickWarningYes()
    {
        WarningDialog.SetActive(false);
    }

    private void OnClickBackToMenuYes()
    {
        AudioSystem.Instance.StopBGM();
        AudioSystem.Instance.PlayBGMByName("SwingJazzMarsMellow");
        
        SceneSystem.Instance.LoadSceneWithCallback(SceneSystem.Instance.GetTitleSceneName(), () =>
        {
            // 씬이 전환된 후 1~2 프레임 기다린 뒤 수행
            StartCoroutine(SetupVideoAfterSceneLoad());
        });

        AllMenuFalse();
    } 
    
    private IEnumerator SetupVideoAfterSceneLoad()
    {
        // 씬 오브젝트들이 완전히 로드될 때까지 기다림
        yield return null;
        yield return new WaitForEndOfFrame();

        // MenuSystem 인스턴스에서 직접 접근 (Find 말고 싱글톤 활용 권장)
        var menuSystem = MenuSystem.Instance;
        if (menuSystem != null && menuSystem.MainMenu != null)
        {
            var videoPlayer = menuSystem.MainMenu.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;

                if (videoPlayer.targetCamera == null && Camera.main != null)
                {
                    videoPlayer.targetCamera = Camera.main;
                    Debug.Log("Main Camera 재연결 완료");
                }

                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += vp =>
                {
                    vp.Play();
                    Debug.Log("VideoPlayer 재생 시작");
                };
            }

            menuSystem.MainMenu.SetActive(true); // 혹시 꺼져 있으면 다시 켜기
        }
        else
        {
            Debug.LogError("MenuSystem.Instance 또는 MainMenu를 찾을 수 없음");
        }
    }

    
    private void OnClickBackToMenuNo()
    {
        AllMenuFalse();
        PauseMenu.SetActive(true);
    }

    public void ShowGameoverView()
    {
        GameOverDialog.SetActive(true);
    }
    
    private void OnClickGameOverYes()
    {
        SceneSystem.Instance.LoadSceneWithCallback(SceneSystem.Instance.GetTitleSceneName(), () =>
        {
            // 씬이 전환된 후 1~2 프레임 기다린 뒤 수행
            StartCoroutine(SetupVideoAfterSceneLoad());
        });

        AllMenuFalse();
        GameOverDialog.SetActive(false);
        MainMenu.SetActive(true);
    }

    // 공개 메서드
    public float GetBGMVolume() => _currentBGM;
    public float GetSFXVolume() => _currentSFX;

    // 슬라이더 디버깅용 메서드
    [ContextMenu("Debug Sliders")]
    public void DebugSliders()
    {
        Debug.Log("=== 슬라이더 디버깅 ===");
        
        if (_bgmSlider != null)
        {
            Debug.Log($"BGM Slider - Interactable: {_bgmSlider.interactable}, HandleRect: {_bgmSlider.handleRect != null}, FillRect: {_bgmSlider.fillRect != null}, Value: {_bgmSlider.value}");
            Debug.Log($"BGM Slider - Min: {_bgmSlider.minValue}, Max: {_bgmSlider.maxValue}, Direction: {_bgmSlider.direction}");
        }
        else
        {
            Debug.LogError("BGM Slider가 null입니다!");
        }
        
        if (_sfxSlider != null)
        {
            Debug.Log($"SFX Slider - Interactable: {_sfxSlider.interactable}, HandleRect: {_sfxSlider.handleRect != null}, FillRect: {_sfxSlider.fillRect != null}, Value: {_sfxSlider.value}");
            Debug.Log($"SFX Slider - Min: {_sfxSlider.minValue}, Max: {_sfxSlider.maxValue}, Direction: {_sfxSlider.direction}");
        }
        else
        {
            Debug.LogError("SFX Slider가 null입니다!");
        }

        Debug.Log($"AudioSystem Instance: {AudioSystem.Instance != null}");
        if (AudioSystem.Instance != null)
        {
            Debug.Log($"AudioSystem BGM Volume: {AudioSystem.Instance.BGMVolume}");
            Debug.Log($"AudioSystem SFX Volume: {AudioSystem.Instance.SFXVolume}");
        }
    }

    // 강제로 슬라이더 테스트
    [ContextMenu("Test Slider Values")]
    public void TestSliderValues()
    {
        if (_bgmSlider != null)
        {
            Debug.Log("BGM 슬라이더 테스트 시작");
            _bgmSlider.value = 0.5f;
            Debug.Log($"BGM 슬라이더 값을 0.5로 설정: 실제값 = {_bgmSlider.value}");
        }

        if (_sfxSlider != null)
        {
            Debug.Log("SFX 슬라이더 테스트 시작");
            _sfxSlider.value = 0.7f;
            Debug.Log($"SFX 슬라이더 값을 0.7로 설정: 실제값 = {_sfxSlider.value}");
        }
    }
}