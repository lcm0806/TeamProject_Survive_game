using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DesignPattern;

[System.Serializable]
public class ResolutionOption
{
    public int width;
    public int height;
    public string displayName;

    public ResolutionOption(int w, int h)
    {
        width = w;
        height = h;
        displayName = $"{w} x {h}";
    }
}

public class GraphicsSystem : Singleton<GraphicsSystem>
{
    [Header("그래픽 UI 요소들")]
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    
    [Header("버튼들")]
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _backButton;
    
    // 그래픽 설정 값 저장용
    private bool _isFullscreen = true;
    private int _currentResolutionIndex = 0;
    private int _currentQualityIndex = 2;
    
    // UI 업데이트 중 이벤트 무시용 플래그
    private bool _isUpdatingUI = false;
    
    // 해상도 옵션들
    private List<ResolutionOption> _resolutionOptions;
    
    // PlayerPrefs 키들
    private const string _fullscreenKey = "Fullscreen";
    private const string _resolutionIndexKey = "ResolutionIndex";
    private const string _qualityIndexKey = "QualityIndex";

    void Awake()
    {
        // 해상도 옵션 초기화
        InitializeResolutionOptions();
    }

    void Start()
    {
        LoadGraphicsSettings();
        SetupEventListeners();
        SetupDropdowns();
    }

    /// <summary>
    /// 해상도 옵션 초기화
    /// </summary>
    private void InitializeResolutionOptions()
    {
        _resolutionOptions = new List<ResolutionOption>
        {
            new ResolutionOption(1920, 1080),
            new ResolutionOption(1680, 1050),
            new ResolutionOption(1600, 900),
            new ResolutionOption(1440, 900),
            new ResolutionOption(1366, 768),
            new ResolutionOption(1280, 720),
            new ResolutionOption(1024, 768),
            new ResolutionOption(800, 600)
        };
        
        // 현재 해상도에 가장 가까운 옵션을 찾아 기본값으로 설정
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        
        Debug.Log($"현재 화면 해상도: {currentWidth}x{currentHeight}");
        
        for (int i = 0; i < _resolutionOptions.Count; i++)
        {
            if (_resolutionOptions[i].width == currentWidth && _resolutionOptions[i].height == currentHeight)
            {
                _currentResolutionIndex = i;
                break;
            }
        }
    }

    /// <summary>
    /// 드롭다운 설정
    /// </summary>
    private void SetupDropdowns()
    {
        _isUpdatingUI = true; // UI 업데이트 시작
        
        // 해상도 드롭다운 설정
        if (_resolutionDropdown != null)
        {
            _resolutionDropdown.ClearOptions();
            List<string> resolutionNames = new List<string>();
            foreach (var resolution in _resolutionOptions)
            {
                resolutionNames.Add(resolution.displayName);
            }
            _resolutionDropdown.AddOptions(resolutionNames);
            _resolutionDropdown.value = _currentResolutionIndex;
            _resolutionDropdown.RefreshShownValue();
        }
        
        // 품질 드롭다운 설정
        if (_qualityDropdown != null)
        {
            _qualityDropdown.ClearOptions();
            List<string> qualityNames = new List<string>();
            string[] qualityLevels = QualitySettings.names;
            foreach (string quality in qualityLevels)
            {
                qualityNames.Add(quality);
            }
            _qualityDropdown.AddOptions(qualityNames);
            _qualityDropdown.value = _currentQualityIndex;
            _qualityDropdown.RefreshShownValue();
        }
        
        _isUpdatingUI = false; // UI 업데이트 종료
    }

    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    void SetupEventListeners()
    {
        // 그래픽 설정 이벤트만 등록 (버튼 이벤트 제거)
        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
    
        if (_resolutionDropdown != null)
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    
        if (_qualityDropdown != null)
            _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    /// <summary>
    /// 이벤트 리스너 제거
    /// </summary>
    private void RemoveEventListeners()
    {
        if (_fullscreenToggle != null)
            _fullscreenToggle.onValueChanged.RemoveAllListeners();
        
        if (_resolutionDropdown != null)
            _resolutionDropdown.onValueChanged.RemoveAllListeners();
        
        if (_qualityDropdown != null)
            _qualityDropdown.onValueChanged.RemoveAllListeners();
        
        if (_okButton != null)
            _okButton.onClick.RemoveAllListeners();
        
        if (_backButton != null)
            _backButton.onClick.RemoveAllListeners();
    }

    // ========== 그래픽 설정 메서드들 ==========
    
    public void OnFullscreenToggleChanged(bool isFullscreen)
    {
        if (_isUpdatingUI) return;
        
        _isFullscreen = isFullscreen;
        Debug.Log($"전체화면 설정 변경: {isFullscreen}");
        
        // 전체화면 설정은 즉시 적용
        StartCoroutine(ApplyFullscreenMode(isFullscreen));
    }
    
    public void OnResolutionChanged(int index)
    {
        if (_isUpdatingUI) return;
        
        if (index >= 0 && index < _resolutionOptions.Count)
        {
            _currentResolutionIndex = index;
            Debug.Log($"해상도 설정 변경: {_resolutionOptions[index].displayName}");
            
            // 해상도 설정도 즉시 적용
            StartCoroutine(ApplyResolutionImmediate(index));
        }
    }
    
    public void OnQualityChanged(int index)
    {
        if (_isUpdatingUI) return;
        
        _currentQualityIndex = index;
        Debug.Log($"품질 설정 변경: {QualitySettings.names[index]}");
        
        // 품질 설정 즉시 적용
        QualitySettings.SetQualityLevel(index, true);
    }

    // ========== 버튼 이벤트 메서드들 ==========
    
    public void OnOKButtonClicked()
    {
        Debug.Log("OK 버튼 클릭 - 설정 저장");
        SaveGraphicsSettings();
        Debug.Log("그래픽 설정이 저장되었습니다.");
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Back 버튼 클릭 - 설정 복원 시작");
        LoadGraphicsSettings();
        Debug.Log("그래픽 설정이 이전 상태로 되돌려졌습니다.");
    }

    // ========== 설정 적용 메서드들 ==========
    
    private IEnumerator ApplyFullscreenMode(bool fullscreen)
    {
        Debug.Log($"전체화면 모드 즉시 변경: {fullscreen}");
        
        if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _resolutionOptions.Count)
        {
            var resolution = _resolutionOptions[_currentResolutionIndex];
            
            if (!fullscreen)
            {
                // 창모드로 전환
                Screen.fullScreen = false;
                yield return new WaitForSeconds(0.1f);
                Screen.SetResolution(resolution.width, resolution.height, false);
            }
            else
            {
                // 전체화면으로 전환
                Screen.SetResolution(resolution.width, resolution.height, true);
            }
        }
        else
        {
            Screen.fullScreen = fullscreen;
        }
        
        yield return new WaitForSeconds(0.2f);
        Debug.Log($"전체화면 모드 변경 완료 - 현재: {Screen.fullScreen}");
    }
    
    private IEnumerator ApplyResolutionImmediate(int index)
    {
        if (index >= 0 && index < _resolutionOptions.Count)
        {
            var resolution = _resolutionOptions[index];
            Debug.Log($"해상도 즉시 변경: {resolution.width}x{resolution.height}");
            
            Screen.SetResolution(resolution.width, resolution.height, _isFullscreen);
            
            yield return new WaitForSeconds(0.2f);
            Debug.Log($"해상도 변경 완료 - 현재: {Screen.width}x{Screen.height}");
        }
    }
    
    private IEnumerator ApplyGraphicsSettingsCoroutine()
    {
        // 전체화면 모드 변경이 필요한 경우
        bool fullscreenChanged = Screen.fullScreen != _isFullscreen;
        
        // 품질 설정 먼저 적용
        Debug.Log($"품질 레벨 적용: {_currentQualityIndex} ({QualitySettings.names[_currentQualityIndex]})");
        QualitySettings.SetQualityLevel(_currentQualityIndex, true);
        
        yield return null; // 한 프레임 대기
        
        // 해상도와 전체화면 설정 적용
        if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _resolutionOptions.Count)
        {
            var resolution = _resolutionOptions[_currentResolutionIndex];
            
            // 전체화면 모드가 변경되는 경우 특별 처리
            if (fullscreenChanged)
            {
                Debug.Log($"전체화면 모드 변경: {_isFullscreen}");
                
                // 창모드로 전환하는 경우
                if (!_isFullscreen)
                {
                    Screen.fullScreen = false;
                    yield return new WaitForSeconds(0.1f); // 모드 전환 대기
                    Screen.SetResolution(resolution.width, resolution.height, false);
                }
                // 전체화면으로 전환하는 경우
                else
                {
                    Screen.SetResolution(resolution.width, resolution.height, true);
                    yield return new WaitForSeconds(0.1f); // 모드 전환 대기
                }
            }
            else
            {
                // 전체화면 모드 변경이 없는 경우
                Debug.Log($"해상도 적용: {resolution.width}x{resolution.height}, 전체화면: {_isFullscreen}");
                Screen.SetResolution(resolution.width, resolution.height, _isFullscreen);
            }
        }
        
        // 설정 저장
        SaveGraphicsSettings();
        
        // 적용 완료 확인
        yield return new WaitForSeconds(0.5f);
        VerifyAppliedSettings();
    }
    
    private IEnumerator ApplyGraphicsSettingsDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        ApplyGraphicsSettings();
    }
    
    private void ApplyGraphicsSettings()
    {
        // 품질 설정 적용
        QualitySettings.SetQualityLevel(_currentQualityIndex, true);
        
        // 해상도와 전체화면 설정 적용
        if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _resolutionOptions.Count)
        {
            var resolution = _resolutionOptions[_currentResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, _isFullscreen);
        }
    }
    
    /// <summary>
    /// 설정이 제대로 적용되었는지 확인
    /// </summary>
    private void VerifyAppliedSettings()
    {
        Debug.Log("=== 그래픽 설정 적용 확인 ===");
        Debug.Log($"목표 전체화면: {_isFullscreen} → 실제: {Screen.fullScreen}");
        Debug.Log($"목표 해상도: {_resolutionOptions[_currentResolutionIndex].displayName} → 실제: {Screen.width}x{Screen.height}");
        Debug.Log($"목표 품질: {QualitySettings.names[_currentQualityIndex]} → 실제: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        
        // 적용이 안 된 경우 재시도
        if (Screen.fullScreen != _isFullscreen || 
            Screen.width != _resolutionOptions[_currentResolutionIndex].width ||
            Screen.height != _resolutionOptions[_currentResolutionIndex].height)
        {
            Debug.LogWarning("설정이 제대로 적용되지 않았습니다. 재시도합니다.");
            StartCoroutine(RetryApplySettings());
        }
    }
    
    private IEnumerator RetryApplySettings()
    {
        yield return new WaitForSeconds(0.5f);
        ApplyGraphicsSettings();
    }

    // ========== 설정 저장/로드 메서드들 ==========

    public void SaveGraphicsSettings()
    {
        PlayerPrefs.SetInt(_fullscreenKey, _isFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(_resolutionIndexKey, _currentResolutionIndex);
        PlayerPrefs.SetInt(_qualityIndexKey, _currentQualityIndex);
        
        PlayerPrefs.Save();
        
        Debug.Log($"그래픽 설정 저장됨 - 전체화면: {_isFullscreen}, " +
                  $"해상도: {_resolutionOptions[_currentResolutionIndex].displayName}, " +
                  $"품질: {QualitySettings.names[_currentQualityIndex]}");
    }

    public void LoadGraphicsSettings()
    {
        _isFullscreen = PlayerPrefs.GetInt(_fullscreenKey, 1) == 1;
        _currentResolutionIndex = PlayerPrefs.GetInt(_resolutionIndexKey, 0);
        _currentQualityIndex = PlayerPrefs.GetInt(_qualityIndexKey, 2);
        
        // 범위 체크
        _currentResolutionIndex = Mathf.Clamp(_currentResolutionIndex, 0, _resolutionOptions.Count - 1);
        _currentQualityIndex = Mathf.Clamp(_currentQualityIndex, 0, QualitySettings.names.Length - 1);
        
        Debug.Log($"그래픽 설정 로드됨 - 전체화면: {_isFullscreen}, " +
                  $"해상도: {_resolutionOptions[_currentResolutionIndex].displayName}, " +
                  $"품질: {QualitySettings.names[_currentQualityIndex]}");
        
        UpdateGraphicsUI();
    }
    
    private void UpdateGraphicsUI()
    {
        _isUpdatingUI = true;
        
        if (_fullscreenToggle != null)
        {
            _fullscreenToggle.isOn = _isFullscreen;
        }
        
        if (_resolutionDropdown != null)
        {
            _resolutionDropdown.value = _currentResolutionIndex;
            _resolutionDropdown.RefreshShownValue();
        }
        
        if (_qualityDropdown != null)
        {
            _qualityDropdown.value = _currentQualityIndex;
            _qualityDropdown.RefreshShownValue();
        }
        
        _isUpdatingUI = false;
    }

    // ========== 공개 메서드들 ==========
    
    public bool IsFullscreen() => _isFullscreen;
    
    public ResolutionOption GetCurrentResolution()
    {
        if (_currentResolutionIndex >= 0 && _currentResolutionIndex < _resolutionOptions.Count)
        {
            return _resolutionOptions[_currentResolutionIndex];
        }
        return _resolutionOptions[0];
    }
    
    public int GetCurrentQuality() => _currentQualityIndex;
    
    public void SetFullscreen(bool fullscreen)
    {
        _isFullscreen = fullscreen;
        StartCoroutine(ApplyGraphicsSettingsCoroutine());
        UpdateGraphicsUI();
    }
    
    public void SetResolution(int width, int height)
    {
        for (int i = 0; i < _resolutionOptions.Count; i++)
        {
            if (_resolutionOptions[i].width == width && _resolutionOptions[i].height == height)
            {
                _currentResolutionIndex = i;
                StartCoroutine(ApplyGraphicsSettingsCoroutine());
                UpdateGraphicsUI();
                break;
            }
        }
    }
    
    public void SetQuality(int qualityLevel)
    {
        if (qualityLevel >= 0 && qualityLevel < QualitySettings.names.Length)
        {
            _currentQualityIndex = qualityLevel;
            QualitySettings.SetQualityLevel(qualityLevel, true);
            UpdateGraphicsUI();
        }
    }

    void OnDestroy()
    {
        RemoveEventListeners();
    }
    
    // 디버깅용 메서드
    [ContextMenu("Debug Current Settings")]
    void DebugCurrentSettings()
    {
        Debug.Log($"=== 현재 그래픽 설정 ===");
        Debug.Log($"전체화면: {_isFullscreen} (실제: {Screen.fullScreen})");
        Debug.Log($"해상도: {_resolutionOptions[_currentResolutionIndex].displayName} (실제: {Screen.width}x{Screen.height})");
        Debug.Log($"품질: {QualitySettings.names[_currentQualityIndex]} (실제: {QualitySettings.names[QualitySettings.GetQualityLevel()]})");
        Debug.Log($"지원되는 해상도: {Screen.resolutions.Length}개");
        foreach (var res in Screen.resolutions)
        {
            Debug.Log($"  - {res.width}x{res.height} @ {res.refreshRateRatio.value:F2}Hz");
        }
    }
}