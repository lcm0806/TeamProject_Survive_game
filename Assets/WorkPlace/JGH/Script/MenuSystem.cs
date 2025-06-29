using UnityEngine;
using UnityEngine.UI;
using DesignPattern;

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
    private Button _settingOkButton;
    private Button _settingBackButton;

    [Header("일시정지 메뉴")] 
    public GameObject PauseMenu;
    private Button _pauseSetButton;
    private Button _pauseExitButton;

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
    
    [Header("경고 메뉴")]
    public GameObject WarningDialog;
    private Button _warningYesButton;
    
    // 메뉴 상태 관리
    private GameObject _currentOpenMenu = null;
    private bool _isMenuOpen = false;
    
    // 설정 백업용 변수들
    private float _backupBGMVolume;
    private float _backupSFXVolume;
    private bool _backupFullscreen;
    private int _backupQualityIndex;

    void Awake()
    {
        SingletonInit();
    }
    
    void Update()
    {
        // ESC 키 입력 처리
        HandleEscapeInput();
    } 
    
    /// <summary>
    /// ESC 키 입력 처리
    /// </summary>
    private void HandleEscapeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string currentSceneName = GetCurrentSceneName();
            
            // TitleScene에서는 ESC 무시
            if (currentSceneName == "TitleScene" || currentSceneName == "MainMenuScene")
            {
                return;
            }
            
            // 이미 메뉴가 열려있는 경우
            if (_isMenuOpen)
            {
                // 일시정지 메뉴가 열려있다면 닫기 (게임 재개)
                if (_currentOpenMenu == PauseMenu)
                {
                    ResumeGame();
                }
                // 설정 메뉴가 열려있다면 설정 취소 (이전 상태로 복구)
                else if (_currentOpenMenu == SettingMenu)
                {
                    OnSettingBackButtonClick();
                }
                // 다른 메뉴가 열려있다면 닫기
                else
                {
                    CloseCurrentMenu();
                }
            }
            // 메뉴가 열려있지 않다면 일시정지 메뉴 열기
            else
            {
                PauseGame();
            }
        }
    }
    
    /// <summary>
    /// 게임 일시정지 및 일시정지 메뉴 열기
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("게임 일시정지");
        
        // GameSystem 싱글톤 참조로 게임 일시정지
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.Pause();
        }
        
        // 일시정지 메뉴 열기
        OpenMenu(PauseMenu);
    }

    /// <summary>
    /// 게임 재개 및 일시정지 메뉴 닫기 
    /// </summary>
    public void ResumeGame()
    {
        Debug.Log("게임 재개");
        
        // 메뉴 닫기
        CloseCurrentMenu();
        
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.Resume();
        }
    }
    
    private void OnPauseSetButtonClick()
    {
        if (IsAnyMenuOpen() && _currentOpenMenu != PauseMenu) return;
        Debug.Log("일시정지 메뉴 - 설정 버튼 클릭");
        
        // 설정 메뉴를 열기 전에 현재 설정값들을 백업
        BackupCurrentSettings();
        
        // 일시정지 메뉴를 닫고 설정 메뉴 열기
        CloseCurrentMenu();
        OpenMenu(SettingMenu);
    }
    
    /// <summary>
    /// 설정 메뉴를 열기 전에 현재 설정값들을 백업
    /// </summary>
    private void BackupCurrentSettings()
    {
        Debug.Log("설정값 백업 시작");
        
        // AudioSystem 설정 백업
        if (AudioSystem.Instance != null)
        {
            _backupBGMVolume = AudioSystem.Instance.BGMVolume;
            _backupSFXVolume = AudioSystem.Instance.SFXVolume;
            Debug.Log($"오디오 설정 백업 - BGM: {_backupBGMVolume:F2}, SFX: {_backupSFXVolume:F2}");
        }
        
        // GraphicsSystem 설정 백업
        if (GraphicsSystem.Instance != null)
        {
            _backupFullscreen = GraphicsSystem.Instance.IsFullscreen();
            _backupQualityIndex = GraphicsSystem.Instance.GetCurrentQuality();
            
            Debug.Log($"그래픽 설정 백업 - 전체화면: {_backupFullscreen}, 품질: {_backupQualityIndex}");
        }
        
        Debug.Log("설정값 백업 완료");
    }

    /// <summary>
    /// 백업된 설정값들로 복구
    /// </summary>
    private void RestoreBackupSettings()
    {
        Debug.Log("설정값 복구 시작");
        
        // AudioSystem 설정 복구
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.OnBGMVolumeChanged(_backupBGMVolume);
            AudioSystem.Instance.OnSFXVolumeChanged(_backupSFXVolume);
            Debug.Log($"오디오 설정 복구 - BGM: {_backupBGMVolume:F2}, SFX: {_backupSFXVolume:F2}");
        }
        
        // GraphicsSystem 설정 복구
        if (GraphicsSystem.Instance != null)
        {
            GraphicsSystem.Instance.SetFullscreen(_backupFullscreen);
            GraphicsSystem.Instance.SetQuality(_backupQualityIndex);
            Debug.Log($"그래픽 설정 복구 - 전체화면: {_backupFullscreen}, 품질: {_backupQualityIndex}");
        }
        
        Debug.Log("설정값 복구 완료");
    }
    
    
    
    private void OnPauseExitButtonClick()
    {
        if (IsAnyMenuOpen() && _currentOpenMenu != PauseMenu) return;
        Debug.Log("일시정지 메뉴 - 메인 메뉴로 나가기");
    
    
        // 메뉴 닫기
        CloseCurrentMenu();
        OpenMenu(ExitMenu);
    }
    

    private void PauseMenuButtons()
    {
        if (PauseMenu != null)
        {
            _pauseSetButton?.onClick.AddListener(OnPauseSetButtonClick);
            _pauseExitButton?.onClick.AddListener(OnPauseExitButtonClick);
        }
    }
    
    /// <summary>
    /// 현재 씬 이름 가져오기 
    /// </summary>
    private string GetCurrentSceneName()
    {
        if (SceneSystem.Instance != null)
        {
            return SceneSystem.Instance.GetCurrentSceneName();
        }
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
    void Start()
    {
        if (MainMenu != null)
        {
            _mainStartButton = MainMenu.transform.Find("StartButton")?.GetComponent<Button>();
            _mainSetButton = MainMenu.transform.Find("SettingButton")?.GetComponent<Button>();
            _mainContinueButton = MainMenu.transform.Find("ContinueButton")?.GetComponent<Button>();
            
            // 만든이들 버튼 찾기 (여러 가능한 이름 시도)
            _mainCreatorsButton = MainMenu.transform.Find("CreatorButton")?.GetComponent<Button>();
            if (_mainCreatorsButton == null)
                _mainCreatorsButton = MainMenu.transform.Find("CreatorsButton")?.GetComponent<Button>();
            if (_mainCreatorsButton == null)
                _mainCreatorsButton = MainMenu.transform.Find("MakerButton")?.GetComponent<Button>();
            
            _mainExitButton = MainMenu.transform.Find("ExitButton")?.GetComponent<Button>();
            
            // 디버그 로그
            Debug.Log($"만든이들 버튼 찾기 결과: {(_mainCreatorsButton != null ? "성공" : "실패")}");
            if (_mainCreatorsButton == null)
            {
                Debug.LogError("CreatorButton을 찾을 수 없습니다!");
                Debug.Log("MainMenu의 자식 오브젝트들:");
                for (int i = 0; i < MainMenu.transform.childCount; i++)
                {
                    Debug.Log($"  - {MainMenu.transform.GetChild(i).name}");
                }
            }
            
            MainMenuButtons();
        }

        if (NewGameDialog != null)
        {
            _newGameOkButton = NewGameDialog.transform.Find("OkButton")?.GetComponent<Button>();
            _newGameBackButton = NewGameDialog.transform.Find("BackButton")?.GetComponent<Button>();
            NewGameDialogButtons();
        }

        if (ContinueDialog != null)
        {
            _continueDialogOkButton = ContinueDialog.transform.Find("OkButton")?.GetComponent<Button>();
            _continueDialogBackButton = ContinueDialog.transform.Find("BackButton")?.GetComponent<Button>();
            ContinueDialogButtons();
        }

        if (CreatorsDialog != null)
        {
            _creatorsYesButton = CreatorsDialog.transform.Find("YesButton")?.GetComponent<Button>();
            
            Debug.Log($"만든이들 다이얼로그 Yes 버튼 찾기 결과: {(_creatorsYesButton != null ? "성공" : "실패")}");
            
            CreatorsMenuButtons();
        }

        if (WarningDialog != null)
        {
            _warningYesButton = WarningDialog.transform.Find("YesButton")?.GetComponent<Button>();
            WarningDialogButtons();
        }

        if (ExitMenu != null)
        {
            _exitBackButton = ExitMenu.transform.Find("BackButton")?.GetComponent<Button>();
            _exitOkButton = ExitMenu.transform.Find("OkButton")?.GetComponent<Button>();
            ExitMenuButtons();
        }

        if (SettingMenu != null)
        {
            _settingBackButton = SettingMenu.transform.Find("BackButton")?.GetComponent<Button>();
            _settingOkButton = SettingMenu.transform.Find("OkButton")?.GetComponent<Button>();
            SettingMenuButtons();
        }

        if (PauseMenu != null)
        {
            _pauseSetButton = PauseMenu.transform.Find("SettingButton")?.GetComponent<Button>();
            _pauseExitButton = PauseMenu.transform.Find("ExitButton")?.GetComponent<Button>();    
            PauseMenuButtons();
        }
        
        // TODO: TEST
        AudioSystem.Instance.PlayBGMByName("MainBGM");
    }
    
    /// <summary>
    /// 메뉴 열기 (다른 메뉴 차단)
    /// </summary>
    private void OpenMenu(GameObject menuToOpen)
    {
        if (_isMenuOpen) 
        {
            Debug.Log("이미 메뉴가 열려있어서 무시됨");
            return; // 이미 메뉴가 열려있으면 무시
        }
        
        _currentOpenMenu = menuToOpen;
        _isMenuOpen = true;
        menuToOpen.SetActive(true);
        // MainMenu는 계속 활성화 상태 유지
        
        Debug.Log($"메뉴 열림: {menuToOpen.name}");
    }

    /// <summary>
    /// 메뉴 닫기
    /// </summary>
    private void CloseCurrentMenu()
    {
        if (_currentOpenMenu != null)
        {
            Debug.Log($"메뉴 닫힘: {_currentOpenMenu.name}");
            _currentOpenMenu.SetActive(false);
            _currentOpenMenu = null;
        }
        _isMenuOpen = false;
        // MainMenu는 이미 활성화되어 있으므로 별도로 활성화할 필요 없음
    }

    /// <summary>
    /// 메뉴가 열려있는지 확인
    /// </summary>
    private bool IsAnyMenuOpen()
    {
        return _isMenuOpen;
    }

    private void MainMenuButtons()
    {
        if (MainMenu != null)
        {
            _mainStartButton?.onClick.AddListener(OnMainStartButtonClick);
            _mainSetButton?.onClick.AddListener(OnMainSetButtonClick);
            _mainExitButton?.onClick.AddListener(OnMainExitButtonClick);
            _mainContinueButton?.onClick.AddListener(OnMainContinueButtonClick);
            
            if (_mainCreatorsButton != null)
            {
                _mainCreatorsButton.onClick.AddListener(OnMainCreatorButtonClick);
                Debug.Log("만든이들 버튼 이벤트 등록 완료");
            }
            else
            {
                Debug.LogError("만든이들 버튼이 null이어서 이벤트를 등록할 수 없습니다!");
            }
        }
    }

    private void SettingMenuButtons()
    {
        if (SettingMenu != null)
        {
            // 기존 리스너 제거 (중복 방지)
            _settingBackButton?.onClick.RemoveAllListeners();
            _settingOkButton?.onClick.RemoveAllListeners();
        
            // 새로운 리스너 추가
            _settingBackButton?.onClick.AddListener(OnSettingBackButtonClick);
            _settingOkButton?.onClick.AddListener(OnSettingOkButtonClick);
        }
    }

    private void OnSettingBackButtonClick()
    {
        Debug.Log("설정 메뉴 - 뒤로가기 버튼 클릭");
        
        AudioSystem.Instance?.LoadVolumeSettings();
        GraphicsSystem.Instance?.LoadGraphicsSettings();
        // 백업된 설정값들로 복구
        RestoreBackupSettings(); 
        // 메뉴 닫기
        CloseCurrentMenu();
        
        // 게임 씬에서 설정을 열었다면 일시정지 메뉴로 돌아가기
        string currentSceneName = GetCurrentSceneName();
        if (currentSceneName != "TitleScene" && currentSceneName != "MainMenuScene")
        {
            OpenMenu(PauseMenu);
        }
    }

    private void OnSettingOkButtonClick()
    {
        Debug.Log("설정 메뉴 - 확인 버튼 클릭");
    
        // 다른 시스템들의 설정 저장
        AudioSystem.Instance?.SaveVolumeSettings();
        GraphicsSystem.Instance?.SaveGraphicsSettings();
    
        // 메뉴 닫기
        CloseCurrentMenu();
        
        // 게임 씬에서 설정을 열었다면 일시정지 메뉴로 돌아가기
        string currentSceneName = GetCurrentSceneName();
        if (currentSceneName != "TitleScene" && currentSceneName != "MainMenuScene")
        {
            OpenMenu(PauseMenu);
        }
    }
    
    public void CloseSettingMenu()
    {
        CloseCurrentMenu(); // 현재 메뉴만 닫기
    }
    
    private void NewGameDialogButtons()
    {
        if (NewGameDialog != null)
        {
            _newGameOkButton?.onClick.AddListener(() =>
            {
                Debug.Log("새 게임 시작 - OK 버튼");
                CloseCurrentMenu();
                StartNewGame();
            });
            
            _newGameBackButton?.onClick.AddListener(() =>
            {
                Debug.Log("새 게임 취소 - Back 버튼");
                CloseCurrentMenu();
            });
        }
    }

    private void ContinueDialogButtons()
    {
        if (ContinueDialog != null)
        {
            _continueDialogOkButton?.onClick.AddListener(() =>
            {
                Debug.Log("게임 불러오기 시도");
                CloseCurrentMenu();
                
                // FileSystem에 불러오기 요청
                if (FileSystem.Instance != null)
                {
                    FileSystem.Instance.TryLoadGameFromMenu();
                }
                else
                {
                    ShowWarningDialog("파일 시스템을 찾을 수 없습니다!");
                }
            });
            
            _continueDialogBackButton?.onClick.AddListener(() =>
            {
                Debug.Log("불러오기 취소");
                CloseCurrentMenu();
            });
        }
    }

    private void WarningDialogButtons()
    {
        if (WarningDialog != null)
        {
            _warningYesButton?.onClick.AddListener(() =>
            {
                Debug.Log("경고창 닫기");
                CloseCurrentMenu();
            });
        }
    }

    private void ExitMenuButtons()
    {
        if (ExitMenu)
        {
            _exitBackButton?.onClick.AddListener(() =>
            {
                Debug.Log("종료 메뉴 - 뒤로가기");
                CloseCurrentMenu(); // 현재 메뉴만 닫기
            });
            
            _exitOkButton?.onClick.AddListener(() =>
            {
                Debug.Log("게임 종료");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            });
        }
    }

    private void CreatorsMenuButtons()
    {
        if (CreatorsDialog)
        {
            _creatorsYesButton?.onClick.AddListener(() =>
            {
                Debug.Log("만든이들 다이얼로그 닫기");
                CloseCurrentMenu(); // 현재 메뉴만 닫기
            });
        }
    }

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    private void StartNewGame()
    {
        Debug.Log("새 게임 시작 - 게임 씬으로 이동");
        
        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.StopBGM();

        // 쉘터 씬 로드
        if (SceneSystem.Instance != null)
        {
            SceneSystem.Instance.LoadShelterScene();
        }
        else
        {
            Debug.LogError("SceneSystem.Instance가 null입니다. 씬 로드 실패!");
        }

        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.PlaySFXByName("MainSFX");

        // 씬 전환시에만 MainMenu 비활성화
        MainMenu.SetActive(false);
    }

    /// <summary>
    /// 경고창 표시 (외부에서 호출 가능)
    /// </summary>
    public void ShowWarningDialog(string message)
    {
        Debug.Log($"경고창 표시: {message}");
        
        if (WarningDialog != null)
        {
            // 경고 메시지 텍스트 설정 (Text 컴포넌트가 있다면)
            var messageText = WarningDialog.GetComponentInChildren<UnityEngine.UI.Text>();
            if (messageText != null)
            {
                messageText.text = message;
            }
            
            // TMPro Text 컴포넌트 확인
            var tmpText = WarningDialog.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = message;
            }
            
            OpenMenu(WarningDialog);
        }
        else
        {
            Debug.LogError("WarningDialog가 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 게임 불러오기 성공 시 호출 (FileSystem에서 호출)
    /// </summary>
    public void OnLoadGameSuccess()
    {
        Debug.Log("게임 불러오기 성공 - 메인 메뉴 비활성화");
        
        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.StopBGM();
        
        MainMenu.SetActive(false);
    }

    /// <summary>
    /// 메인 메뉴에서 새 게임 버튼 클릭 시 (Start 버튼)
    /// </summary>
    public void OnMainStartButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        
        Debug.Log("새 게임 버튼 클릭");
        OpenMenu(NewGameDialog);
    }

    /// <summary>
    /// 메인 메뉴에서 설정 버튼 클릭 시 UI 나오도록
    /// </summary>
    private void OnMainSetButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        Debug.Log("설정 버튼 클릭");
        
        // 메인 메뉴에서 설정을 열 때도 현재 설정값들을 백업
        BackupCurrentSettings();
        
        OpenMenu(SettingMenu);
    }

    /// <summary>
    /// 메인 메뉴에서 종료 버튼 클릭 시 UI 나오도록
    /// </summary>
    private void OnMainExitButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        Debug.Log("종료 버튼 클릭");
        OpenMenu(ExitMenu);
    }

    /// <summary>
    /// 메인 메뉴에서 계속하기 버튼 클릭 시 
    /// </summary>
    private void OnMainContinueButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        Debug.Log("계속하기 버튼 클릭");
        OpenMenu(ContinueDialog);
    }
    
    /// <summary>
    /// 메인 메뉴에서 만든이들 버튼 클릭 시 
    /// </summary>
    private void OnMainCreatorButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        Debug.Log("만든이들 버튼 클릭!");
        OpenMenu(CreatorsDialog);
    }
}