using UnityEngine;
using UnityEngine.UI;

public class MenuSystem : MonoBehaviour
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
    private Button _PuaseSetButton;
    private Button _PuaseExitButton;

    [Header("종료 메뉴")] 
    public GameObject ExitMenu;
    private Button _exitOkButton;
    private Button _exitBackButton;

    [Header("확인 메뉴")]
    public GameObject NewGameDialog;
    private Button _newGameOkButton;
    private Button _newGamebackButton;
    
    public GameObject ContinueDialog;
    private Button _continueDialogOkButton;
    private Button _continueDialogBackButton;
    
    public GameObject CreatorsDialog;
    private Button _creatorsYesButton;
    
    // 메뉴 상태 관리
    private GameObject _currentOpenMenu = null;
    private bool _isMenuOpen = false;
    
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

        if (CreatorsDialog != null)
        {
            _creatorsYesButton = CreatorsDialog.transform.Find("YesButton")?.GetComponent<Button>();
            
            Debug.Log($"만든이들 다이얼로그 Yes 버튼 찾기 결과: {(_creatorsYesButton != null ? "성공" : "실패")}");
            
            CreatorsMenuButtons();
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
        
        // 다른 시스템들의 설정 복원
        AudioSystem.Instance?.LoadVolumeSettings();
        GraphicsSystem.Instance?.LoadGraphicsSettings();
    
        // 메뉴 닫기
        CloseCurrentMenu();
    }

    private void OnSettingOkButtonClick()
    {
        Debug.Log("설정 메뉴 - 확인 버튼 클릭");
    
        // 다른 시스템들의 설정 저장
        AudioSystem.Instance?.SaveVolumeSettings();
        GraphicsSystem.Instance?.SaveGraphicsSettings();
    
        // 메뉴 닫기
        CloseCurrentMenu();
    }
    
    public void CloseSettingMenu()
    {
        CloseCurrentMenu(); // 현재 메뉴만 닫기
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

    /// <summary>
    /// 게임 시작 버튼 누를시
    /// </summary>
    public void OnMainStartButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        
        Debug.Log("게임 시작 버튼 클릭");
        
        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.StopBGM();

        // 쉘터 씬 로드
        if (SceneSystem.Instance != null) SceneSystem.Instance.LoadShelterScene();

        // TODO: TEST
        if (AudioSystem.Instance != null) AudioSystem.Instance.PlaySFXByName("MainSFX");

        // 씬 전환시에만 MainMenu 비활성화
        MainMenu.SetActive(false);
    }

    /// <summary>
    /// 메인 메뉴에서 설정 버튼 클릭 시 UI 나오도록
    /// </summary>
    private void OnMainSetButtonClick()
    {
        if (IsAnyMenuOpen()) return; // 메뉴가 열려있으면 무시
        Debug.Log("설정 버튼 클릭");
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
        OpenMenu(NewGameDialog);
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