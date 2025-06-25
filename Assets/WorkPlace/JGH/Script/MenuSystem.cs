using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSystem : MonoBehaviour
{
    [Header("메인 메뉴")]
    public GameObject MainMenu;
    private Button _mainStartButton;
    private Button _mainSetButton;
    private Button _mainExitButton;
    private Button _mainSaveButton;
    private Button _mainLoadButton;

    [Header("설정 메뉴")]
    public GameObject SettingMenu;
    private Button _settingOkButton;
    private Button _settingBackButton;

    [Header("슬롯 패널")]
    public GameObject SlotPanel;
    private Button[] _slotButtons = new Button[5];
    private Button[] _slotDeleteButtons = new Button[5];
    private TextMeshProUGUI[] _slotTexts = new TextMeshProUGUI[5];
    private Button _backButton;
    private int slotIndexToDelete = -1;

    [Header("확인 대화상자")]
    public GameObject ConfirmDialog;
    private TextMeshProUGUI _confirmText;
    private Button _confirmYesButton;
    private Button _confirmNoButton;

    [Header("종료 메뉴")]
    public GameObject ExitMenu;
    private Button _exitOkButton;
    private Button _exitBackButton;

    private bool isSaveMode = false;
    private int selectedSlotIndex = -1;

    private TextMeshProUGUI _menuTitleText;

    // ESC 입력 씬별 활성화
    private bool escActive = true;
    private bool isInitialized = false;

    private enum ConfirmType
    {
        None, LoadGame, DeleteSlot, OverwriteSave, SaveConfirm, Alert
    }
    private ConfirmType currentConfirmType = ConfirmType.None;

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
        InitializeUI();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        escActive = false;
        isInitialized = false;
        StartCoroutine(ReinitializeUIAfterSceneLoad());
    }
    
    private System.Collections.IEnumerator ReinitializeUIAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.2f); // 딜레이 증가
        
        FindUIElements();
        InitializeUI();
        ConfigureMenuForCurrentScene();
        
        escActive = true; // ESC 활성화
        isInitialized = true;
        
        Debug.Log($"MenuSystem 초기화 완료 - 씬: {GetCurrentSceneName()}");
    }

    void Update()
    {
        // ESC 키 처리를 더 안전하게
        if (escActive && isInitialized && Input.GetKeyDown(KeyCode.Escape))
        {
            string currentScene = GetCurrentSceneName();
            if (!IsTitleScene(currentScene))
            {
                HandleEscapeInGame();
            }
        }
    }

    private void HandleEscapeInGame()
    {
        // 확인 대화상자가 열려있으면 무시
        if (ConfirmDialog != null && ConfirmDialog.activeSelf)
        {
            return;
        }

        // 슬롯 패널이 열려있으면 닫기
        if (SlotPanel != null && SlotPanel.activeSelf)
        {
            ReturnToMainMenu();
            return;
        }

        // 설정 메뉴가 열려있으면 닫기
        if (SettingMenu != null && SettingMenu.activeSelf)
        {
            SettingMenu.SetActive(false);
            MainMenu?.SetActive(true);
            SetMenuTitle("일시정지"); // ESC로 돌아올 때 일시정지로 설정
            return;
        }

        // 종료 메뉴가 열려있으면 닫기
        if (ExitMenu != null && ExitMenu.activeSelf)
        {
            ExitMenu.SetActive(false);
            MainMenu?.SetActive(true);
            SetMenuTitle("일시정지"); // ESC로 돌아올 때 일시정지로 설정
            return;
        }

        // 메인 메뉴가 열려있으면 닫기
        if (MainMenu != null && MainMenu.activeSelf)
        {
            MainMenu.SetActive(false);
            if (GameSystem.Instance != null)
            {
                GameSystem.Instance.Resume();
            }
            return;
        }

        // 아무것도 열려있지 않으면 메인 메뉴 열기
        if (MainMenu != null)
        {
            SetMenuTitle("일시정지"); // ESC로 메인메뉴 열 때 일시정지로 설정
            MainMenu.SetActive(true);
            if (GameSystem.Instance != null)
            {
                // GameSystem의 Pause 메서드 호출하지 않고 직접 처리
                Time.timeScale = 0f;
            }
            Debug.Log("ESC로 메인메뉴 열림 - 타이틀을 '일시정지'로 변경");
        }
    }

    private void FindUIElements()
    {
        // MainMenu
        if (MainMenu == null)
            MainMenu = GameObject.Find("MainMenu") ?? FindInCanvas("MainMenu");
        // SettingMenu
        if (SettingMenu == null)
            SettingMenu = GameObject.Find("SettingMenu") ?? FindInCanvas("SettingMenu");
        // ExitMenu
        if (ExitMenu == null)
            ExitMenu = GameObject.Find("ExitMenu") ?? FindInCanvas("ExitMenu");
        // SlotPanel
        if (SlotPanel == null)
            SlotPanel = GameObject.Find("SlotPanel") ?? FindInCanvas("SlotPanel");
        // ConfirmDialog
        if (ConfirmDialog == null)
            ConfirmDialog = GameObject.Find("ConfirmDialog") ?? FindInCanvas("ConfirmDialog");
        
        Debug.Log($"UI 요소 찾기 완료 - MainMenu: {MainMenu != null}, SlotPanel: {SlotPanel != null}");
    }
    
    private GameObject FindInCanvas(string name)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var t = canvas.transform.Find(name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private void InitializeUI()
    {
        RemoveAllListeners();
        MainMenuButtons();
        SettingMenuButtons();
        ExitMenuButtons();
        SlotPanelButtons();
        ConfirmDialogButtons();
        ConfigureMenuForCurrentScene();
    }

    private void ConfigureMenuForCurrentScene()
    {
        string currentScene = GetCurrentSceneName();
        bool isTitle = IsTitleScene(currentScene);

        // 타이틀 텍스트 찾기
        Transform t = MainMenu?.transform.Find("TitleText") ??
            MainMenu?.transform.Find("Title") ??
            MainMenu?.transform.Find("MenuTitle");
        if (t != null)
        {
            _menuTitleText = t.GetComponent<TextMeshProUGUI>();
            if (_menuTitleText != null)
                _menuTitleText.text = isTitle ? "LOGO" : "일시정지";
        }
        
        // 버튼 활성화/비활성화
        if (_mainStartButton != null)
            _mainStartButton.gameObject.SetActive(isTitle);
        if (_mainSaveButton != null)
            _mainSaveButton.gameObject.SetActive(!isTitle);
    }
    
    private void SetMenuTitle(string title)
    {
        // 메인 메뉴의 타이틀 텍스트 찾기 (더 많은 경우의 수 추가)
        if (_menuTitleText == null && MainMenu != null)
        {
            Transform t = MainMenu.transform.Find("Logo/Title") ??  // Logo 안의 Title 먼저 찾기
                MainMenu.transform.Find("Logo") ??
                MainMenu.transform.Find("TitleText") ??
                MainMenu.transform.Find("Title") ??
                MainMenu.transform.Find("MenuTitle");
            
            if (t != null)
            {
                _menuTitleText = t.GetComponent<TextMeshProUGUI>();
                Debug.Log($"메인메뉴 타이틀 텍스트 찾음: {t.name}");
            }
            
            // Logo 안의 Title을 찾지 못했다면 Logo 오브젝트 안에서 재귀적으로 찾기
            if (_menuTitleText == null)
            {
                Transform logoTransform = MainMenu.transform.Find("Logo");
                if (logoTransform != null)
                {
                    // Logo 하위의 모든 TextMeshProUGUI 컴포넌트 찾기
                    TextMeshProUGUI[] texts = logoTransform.GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in texts)
                    {
                        if (text.name.Contains("Title") || text.name.Contains("title") || 
                            text.name.Contains("Text") || text.name.Contains("Label"))
                        {
                            _menuTitleText = text;
                            Debug.Log($"Logo 하위에서 타이틀 텍스트 찾음: {text.name}");
                            break;
                        }
                    }
                    
                    // 여전히 찾지 못했다면 첫 번째 TextMeshProUGUI 사용
                    if (_menuTitleText == null && texts.Length > 0)
                    {
                        _menuTitleText = texts[0];
                        Debug.Log($"Logo 하위 첫 번째 텍스트 사용: {texts[0].name}");
                    }
                }
            }
        }
        
        if (_menuTitleText != null)
        {
            _menuTitleText.text = title;
            Debug.Log($"메인메뉴 타이틀 변경: {title}");
        }
        else
        {
            Debug.LogWarning("메인메뉴 타이틀 텍스트를 찾을 수 없습니다!");
        }

        // 슬롯 패널의 타이틀 텍스트도 찾아서 변경
        if (SlotPanel != null)
        {
            Transform slotTitle = SlotPanel.transform.Find("Logo/Title") ??  // Logo 안의 Title 먼저 찾기
                SlotPanel.transform.Find("TitleText") ??
                SlotPanel.transform.Find("Title") ??
                SlotPanel.transform.Find("MenuTitle") ??
                SlotPanel.transform.Find("LOGO");
            
            if (slotTitle != null)
            {
                TextMeshProUGUI slotTitleText = slotTitle.GetComponent<TextMeshProUGUI>();
                if (slotTitleText != null)
                {
                    slotTitleText.text = title;
                    Debug.Log($"슬롯 패널 타이틀 변경: {title}");
                }
            }
            else
            {
                // Logo 하위에서 재귀적으로 찾기
                Transform logoTransform = SlotPanel.transform.Find("Logo");
                if (logoTransform != null)
                {
                    TextMeshProUGUI[] texts = logoTransform.GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in texts)
                    {
                        if (text.name.Contains("Title") || text.name.Contains("title"))
                        {
                            text.text = title;
                            Debug.Log($"슬롯패널 Logo 하위 타이틀 변경: {text.name} -> {title}");
                            break;
                        }
                    }
                }
            }
        }
    }

    private void RemoveAllListeners()
    {
        _mainStartButton?.onClick.RemoveAllListeners();
        _mainSaveButton?.onClick.RemoveAllListeners();
        _mainLoadButton?.onClick.RemoveAllListeners();
        _mainSetButton?.onClick.RemoveAllListeners();
        _mainExitButton?.onClick.RemoveAllListeners();
        _settingOkButton?.onClick.RemoveAllListeners();
        _settingBackButton?.onClick.RemoveAllListeners();
        _exitOkButton?.onClick.RemoveAllListeners();
        _exitBackButton?.onClick.RemoveAllListeners();
        for (int i = 0; i < 5; i++)
        {
            _slotButtons[i]?.onClick.RemoveAllListeners();
            _slotDeleteButtons[i]?.onClick.RemoveAllListeners();
        }
        _backButton?.onClick.RemoveAllListeners();
        _confirmYesButton?.onClick.RemoveAllListeners();
        _confirmNoButton?.onClick.RemoveAllListeners();
    }

    private void MainMenuButtons()
    {
        if (MainMenu != null)
        {
            _mainStartButton = FindButton(MainMenu, "StartButton");
            _mainSaveButton = FindButton(MainMenu, "SaveButton");
            _mainLoadButton = FindButton(MainMenu, "LoadButton");
            _mainSetButton = FindButton(MainMenu, "SettingButton");
            _mainExitButton = FindButton(MainMenu, "ExitButton");

            _mainStartButton?.onClick.AddListener(OnMainStartButtonClick);
            _mainSaveButton?.onClick.AddListener(OnMainSaveButtonClick);
            _mainLoadButton?.onClick.AddListener(OnMainLoadButtonClick);
            _mainSetButton?.onClick.AddListener(OnMainSetButtonClick);
            _mainExitButton?.onClick.AddListener(OnMainExitButtonClick);
        }
    }
    
    private Button FindButton(GameObject parent, string name)
    {
        var tf = parent.transform.Find(name);
        return tf ? tf.GetComponent<Button>() : null;
    }

    private void SettingMenuButtons()
    {
        if (SettingMenu)
        {
            _settingBackButton = FindButton(SettingMenu, "BackButton");
            _settingOkButton = FindButton(SettingMenu, "OkButton");
            _settingBackButton?.onClick.AddListener(() =>
            {
                SettingMenu.SetActive(false);
                MainMenu.SetActive(true);
                SetMenuTitle(IsTitleScene(GetCurrentSceneName()) ? "LOGO" : "일시정지");
            });
            _settingOkButton?.onClick.AddListener(() =>
            {
                SettingMenu.SetActive(false);
                MainMenu.SetActive(true);
                SetMenuTitle(IsTitleScene(GetCurrentSceneName()) ? "LOGO" : "일시정지");
            });
        }
    }

    private void ExitMenuButtons()
    {
        if (ExitMenu)
        {
            _exitBackButton = FindButton(ExitMenu, "BackButton");
            _exitOkButton = FindButton(ExitMenu, "OkButton");
            _exitBackButton?.onClick.AddListener(() =>
            {
                ExitMenu.SetActive(false);
                MainMenu.SetActive(true);
                SetMenuTitle(IsTitleScene(GetCurrentSceneName()) ? "LOGO" : "일시정지");
            });
            _exitOkButton?.onClick.AddListener(Application.Quit);
        }
    }

    private void SlotPanelButtons()
    {
        if (SlotPanel == null) return;
        for (int i = 0; i < 5; i++)
        {
            _slotButtons[i] = FindButton(SlotPanel, $"SlotButton{i + 1}");
            _slotDeleteButtons[i] = FindButton(SlotPanel, $"SlotDeleteButton{i + 1}");
            if (_slotButtons[i])
            {
                int idx = i;
                _slotButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
                _slotTexts[i] = _slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            }
            if (_slotDeleteButtons[i])
            {
                int idx = i;
                _slotDeleteButtons[i].onClick.AddListener(() => OnSlotDeleteClicked(idx));
            }
        }
        _backButton = FindButton(SlotPanel, "BackButton");
        _backButton?.onClick.AddListener(OnSlotBackButtonClick);
    }

    private void ConfirmDialogButtons()
    {
        if (ConfirmDialog)
        {
            _confirmText = ConfirmDialog.transform.Find("ConfirmText")?.GetComponent<TextMeshProUGUI>();
            _confirmYesButton = FindButton(ConfirmDialog, "YesButton");
            _confirmNoButton = FindButton(ConfirmDialog, "NoButton");
            _confirmYesButton?.onClick.AddListener(OnConfirmYes);
            _confirmNoButton?.onClick.AddListener(OnConfirmNo);
        }
    }

    // 메뉴 버튼 이벤트
    public void OnMainStartButtonClick()
    {
        if (AudioSystem.Instance != null) AudioSystem.Instance.StopBGM();
        if (SceneSystem.Instance != null) SceneSystem.Instance.LoadShelterScene();
        if (AudioSystem.Instance != null) AudioSystem.Instance.PlaySFXByName("MainSFX");
        MainMenu.SetActive(false);
    }
    
    private void OnMainSaveButtonClick()
    {
        string currentScene = GetCurrentSceneName();
        if (IsTitleScene(currentScene))
        {
            ShowAlert("게임을 시작한 후에 저장이 가능합니다.");
            return;
        }
        isSaveMode = true;
        MainMenu.SetActive(false);
        SlotPanel.SetActive(true);
        SetMenuTitle("게임 저장하기"); // 수정: 타이틀 변경
        UpdateSlotDisplay();
        Debug.Log("저장 모드로 전환, 타이틀을 '게임 저장하기'로 변경");
    }
    
    private void OnMainLoadButtonClick()
    {
        isSaveMode = false;
        MainMenu.SetActive(false);
        SlotPanel.SetActive(true);
        SetMenuTitle("게임 불러오기"); // 수정: 타이틀 변경
        UpdateSlotDisplay();
        Debug.Log("로드 모드로 전환, 타이틀을 '게임 불러오기'로 변경");
    }
    
    private void OnMainSetButtonClick()
    {
        MainMenu.SetActive(false);
        SettingMenu.SetActive(true);
    }
    
    private void OnMainExitButtonClick()
    {
        MainMenu.SetActive(false);
        ExitMenu.SetActive(true);
    }

    // 슬롯 클릭
    private void OnSlotClicked(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        if (isSaveMode) HandleSaveSlot(slotIndex);
        else HandleLoadSlot(slotIndex);
    }
    
    private void HandleSaveSlot(int idx)
    {
        if (FileSystem.Instance == null) return;
        var infos = FileSystem.Instance.GetAllSlotInfo();
        if (!infos[idx].isEmpty)
        {
            currentConfirmType = ConfirmType.OverwriteSave;
            ShowConfirm($"슬롯 {idx + 1}에 이미 저장된 데이터가 있습니다.\n덮어쓰시겠습니까?");
        }
        else
        {
            currentConfirmType = ConfirmType.SaveConfirm;
            ShowConfirm($"슬롯 {idx + 1}에 저장하시겠습니까?"); // 수정: 저장 확인 메시지
        }
    }
    
    private void HandleLoadSlot(int idx)
    {
        if (FileSystem.Instance == null) return;
        var infos = FileSystem.Instance.GetAllSlotInfo();
        if (!infos[idx].isEmpty)
        {
            currentConfirmType = ConfirmType.LoadGame;
            ShowConfirm($"슬롯 {idx + 1}의 데이터를 불러오시겠습니까?\n\n{infos[idx].saveName}\n{GetFormattedDate(infos[idx].saveDate)}");
        }
        else
        {
            currentConfirmType = ConfirmType.Alert;
            ShowAlert($"슬롯 {idx + 1}에 저장된 데이터가 없습니다.");
        }
    }
    
    private void OnSlotDeleteClicked(int idx)
    {
        if (FileSystem.Instance == null) return;
        var infos = FileSystem.Instance.GetAllSlotInfo();
        if (!infos[idx].isEmpty)
        {
            slotIndexToDelete = idx;
            selectedSlotIndex = idx;
            currentConfirmType = ConfirmType.DeleteSlot;
            ShowConfirm($"슬롯 {idx + 1}의 저장 데이터를 삭제하시겠습니까?\n\n{infos[idx].saveName}");
        }
        else
        {
            currentConfirmType = ConfirmType.Alert;
            ShowAlert($"슬롯 {idx + 1}에 삭제할 데이터가 없습니다.");
        }
    }
    
    private void SaveToSlot(int idx)
    {
        string saveName = $"저장 데이터 {idx + 1}";
        if (FileSystem.Instance != null && FileSystem.Instance.SaveGame(idx, saveName))
        {
            UpdateSlotDisplay();
            currentConfirmType = ConfirmType.Alert;
            ShowAlert($"슬롯 {idx + 1}에 저장되었습니다.");
        }
        else
        {
            currentConfirmType = ConfirmType.Alert;
            ShowAlert("저장에 실패했습니다.");
        }
    }
    
    private void LoadFromSlot(int idx)
    {
        if (FileSystem.Instance == null) return;
        try
        {
            bool success = FileSystem.Instance.LoadGame(idx);
            if (success)
            {
                // 메뉴 닫기
                SlotPanel.SetActive(false);
                MainMenu.SetActive(false);
                ConfirmDialog.SetActive(false);
                
                // 게임 재개
                if (GameSystem.Instance != null)
                {
                    GameSystem.Instance.Resume();
                }
                
                // 씬 로드는 FileSystem에서 처리됨
                Debug.Log($"슬롯 {idx + 1} 로드 성공");
            }
            else
            {
                currentConfirmType = ConfirmType.Alert;
                ShowAlert($"슬롯 {idx + 1} 로드에 실패했습니다.\n저장 파일이 없거나 손상되었습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로드 중 오류: {e.Message}");
            currentConfirmType = ConfirmType.Alert;
            ShowAlert("로드 중 오류가 발생했습니다.");
        }
    }
    
    private void UpdateSlotDisplay()
    {
        if (FileSystem.Instance == null) return;
        var infos = FileSystem.Instance.GetAllSlotInfo();
        for (int i = 0; i < 5; i++)
        {
            if (_slotButtons[i] == null || _slotTexts[i] == null) continue;
            var info = infos[i];
            if (info.isEmpty)
            {
                _slotTexts[i].text = $"슬롯 {i + 1}\n비어있음";
                _slotButtons[i].interactable = isSaveMode;
            }
            else
            {
                string dateDisplay = GetFormattedDate(info.saveDate);
                string text = $"슬롯 {i + 1}\n{info.saveName}\n{dateDisplay}\nDay {info.currentDay} | {FormatPlayTime(info.totalPlayTime)}";
                _slotTexts[i].text = text;
                _slotButtons[i].interactable = true;
            }
            _slotTexts[i].fontSize = 4.85f;
            _slotTexts[i].alignment = TMPro.TextAlignmentOptions.Left;
            _slotTexts[i].enableWordWrapping = true;
        }
    }
    
    private string GetFormattedDate(string dateString)
    {
        if (!string.IsNullOrEmpty(dateString))
            if (DateTime.TryParse(dateString, out DateTime d)) return d.ToString("MM/dd HH:mm");
            else return dateString;
        return "날짜 오류";
    }
    
    private string FormatPlayTime(float totalSeconds)
    {
        int hours = Mathf.FloorToInt(totalSeconds / 3600f);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);
        return (hours > 0) ? $"{hours}시간 {minutes}분" : $"{minutes}분";
    }
    
    private void OnSlotBackButtonClick()
    {
        ReturnToMainMenu();
    }
    
    private void ReturnToMainMenu()
    {
        SlotPanel.SetActive(false);
        MainMenu.SetActive(true);
        // 현재 씬이 타이틀이 아니면 일시정지, 타이틀이면 LOGO로 설정
        string currentScene = GetCurrentSceneName();
        SetMenuTitle(IsTitleScene(currentScene) ? "LOGO" : "일시정지");
        Debug.Log($"메인메뉴로 돌아감 - 씬: {currentScene}, 타이틀: {(IsTitleScene(currentScene) ? "LOGO" : "일시정지")}");
    }
    
    // 컨펌/알림
    private void ShowConfirm(string msg)
    {
        if (_confirmText != null) _confirmText.text = msg;
        _confirmYesButton?.gameObject.SetActive(true);
        var yesText = _confirmYesButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (yesText != null) yesText.text = "예";
        _confirmNoButton?.gameObject.SetActive(true);
        ConfirmDialog.SetActive(true);
    }
    
    private void ShowAlert(string msg)
    {
        if (_confirmText != null) _confirmText.text = msg;
        _confirmYesButton?.gameObject.SetActive(true);
        var yesText = _confirmYesButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (yesText != null) yesText.text = "확인";
        _confirmNoButton?.gameObject.SetActive(false);
        ConfirmDialog.SetActive(true);
    }
    
    private void HideConfirmDialog()
    {
        ConfirmDialog.SetActive(false);
        currentConfirmType = ConfirmType.None;
    }
    
    private void OnConfirmYes()
    {
        ConfirmType typeToProcess = currentConfirmType;
        int slotToProcess = selectedSlotIndex;
        int slotToDelete = slotIndexToDelete;
        
        HideConfirmDialog();
        
        switch (typeToProcess)
        {
            case ConfirmType.LoadGame:
                LoadFromSlot(slotToProcess); 
                break;
            case ConfirmType.DeleteSlot:
                if (FileSystem.Instance != null && FileSystem.Instance.DeleteSaveSlot(slotToDelete))
                {
                    UpdateSlotDisplay();
                    ShowAlert($"슬롯 {slotToDelete + 1}이 삭제되었습니다.");
                }
                else
                {
                    ShowAlert("삭제에 실패했습니다.");
                }
                slotIndexToDelete = -1; 
                break;
            case ConfirmType.OverwriteSave:
            case ConfirmType.SaveConfirm:
                SaveToSlot(slotToProcess); 
                break;
            case ConfirmType.Alert: 
                // 아무것도 하지 않음
                break;
        }
        selectedSlotIndex = -1;
    }
    
    private void OnConfirmNo()
    {
        HideConfirmDialog();
        slotIndexToDelete = -1;
        selectedSlotIndex = -1;
    }
    
    private string GetCurrentSceneName()
    {
        return SceneSystem.Instance?.GetCurrentSceneName() ??
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
    private bool IsTitleScene(string sceneName)
    {
        string[] startScenes = { "StartScene", "TitleScene", "MainMenuScene", "MenuScene", "MainMenu", "Title" };
        foreach (string s in startScenes)
            if (sceneName.Equals(s, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}