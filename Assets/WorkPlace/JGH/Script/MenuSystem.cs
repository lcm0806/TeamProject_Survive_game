using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    private Button _mainStartButton;
    private Button _mainSetButton;
    private Button _mainExitButton;
    
    public GameObject ExitMenu;
    private Button _exitOkButton;
    private Button _exitBackButton;
    
    public GameObject SettingMenu;
    private Button _settingOkButton;
    private Button _settingBackButton;
    
    private void Start()
    {
        MainMenuButtons();
        SettingMenuButtons();
        ExitMenuButtons();
        
        AudioSystem.Instance.PlayBGMByName("MainBGM");
        AudioSystem.Instance.PlaySFXByName("MainSFX");
    }
    
    
    
    private void MainMenuButtons()
    {
        // MainMenu 버튼 설정
        if (MainMenu != null)
        {
            
            // Start
            _mainStartButton = MainMenu.transform.Find("StartButton")?.GetComponent<Button>();
            if (_mainStartButton != null)
            {
                _mainStartButton.onClick.AddListener(OnMainStartButtonClick);
            }
            else
            {
                Debug.LogError("StartButton을 찾을 수 없습니다!");
            }
            
            
            
            
            // Set
            _mainSetButton = MainMenu.transform.Find("SettingButton")?.GetComponent<Button>();
            if (_mainSetButton)
            {
                _mainSetButton.onClick.AddListener(OnMainSetButtonClick);
            }
            else
            {
                Debug.LogError("Set 버튼을 찾을 수 없습니다!");
            }
            
            
            
            
            // Exit
            _mainExitButton= MainMenu.transform.Find("ExitButton")?.GetComponent<Button>();
            if (_mainExitButton != null)
            {
                _mainExitButton.onClick.AddListener(OnMainExitButtonClick);
            }
            else
            {
                Debug.LogError("Exit 버튼을 찾을 수 없습니다!");
            }

        }
        else
        {
            Debug.LogError("MainMenu가 할당되지 않았습니다!");
        }
        
    }
    
    

    private void SettingMenuButtons()
    {
        if (SettingMenu != null)
        {
            _settingBackButton = SettingMenu.transform.Find("BackButton")?.GetComponent<Button>();
            if (_settingBackButton)
            {
                _settingBackButton.onClick.AddListener(SettingBackClick);
            }
            
            
            _settingOkButton = SettingMenu.transform.Find("OkButton")?.GetComponent<Button>();
            if (_settingOkButton)
            {
                _settingOkButton.onClick.AddListener(SettingOkClick);
            }
        }
    }
    
    
    
    private void ExitMenuButtons()
    {
        // ExitMenu 버튼 설정
        if (ExitMenu != null)
        {
            
            _exitBackButton = ExitMenu.transform.Find("BackButton")?.GetComponent<Button>();
            if (_exitBackButton)
            {
                _exitBackButton.onClick.AddListener(OnExitBackButtonClick);
            }
            else
            {
                Debug.Log("BackButton 없음");
            }
            
            _exitOkButton = ExitMenu.transform.Find("OkButton")?.GetComponent<Button>();
            if (_exitOkButton)
            {
                _exitOkButton.onClick.AddListener(OnExitOkButtonClick);
            }
            else
            {
                Debug.Log("OkButton 없음");
            }
        }
        else
        {
            Debug.LogError("ExitMenu가 할당되지 않았습니다!");
        }
    }
    
    
    
    
    // 실제 버튼 클릭 시 실행될 메서드들
    private void OnMainStartButtonClick()
    {
        Debug.Log("게임 시작! 씬으로 이동합니다.");
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

    
    
    
    
    private void SettingBackClick()
    {
        MainMenu.SetActive(true);
        SettingMenu.SetActive(false);
    }

    private void SettingOkClick()
    {
        MainMenu.SetActive(true);
        SettingMenu.SetActive(false);
    }

    
    
    
    
    
    
    private void OnExitBackButtonClick()
    {
        MainMenu.SetActive(true);
        ExitMenu.SetActive(false);
    }

    private void OnExitOkButtonClick()
    {
        Application.Quit();
    }
}