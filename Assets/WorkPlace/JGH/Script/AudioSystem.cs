using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using DesignPattern;

[System.Serializable]
public class AudioClipGroup
{
    public string name;
    public AudioClip audioClip;
}

public class AudioSystem : Singleton<AudioSystem>
{
    [Header("UI 요소들")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _bgmVolumeText;
    [SerializeField] private TMP_Text _sfxVolumeText;
    [SerializeField] private Button _okButton;
    [SerializeField] private Button _backButton;
    
    [Header("오디오 설정")]
    [SerializeField] private AudioMixer _audioMixer;
    
    [Header("BGM 클립들")]
    [SerializeField] private List<AudioClipGroup> _bgmClips = new List<AudioClipGroup>();
    
    [Header("SFX 클립들")]
    [SerializeField] private List<AudioClipGroup> _sfxClips = new List<AudioClipGroup>();
    
    // AudioSource 컴포넌트들
    private AudioSource _bgmAudioSource;
    private AudioSource _sfxAudioSource;
    
    // 볼륨 값 저장용
    public float BGMVolume = 0.5f;
    public float SFXVolume = 0.5f;
    
    // PlayerPrefs 키
    public string BGMVolumeKey = "BGMVolume";
    public string SFXVolumeKey = "SFXVolume";

    void Awake()
    {
        SingletonInit();
        
        // AudioSource 컴포넌트 생성
        _bgmAudioSource = gameObject.AddComponent<AudioSource>();
        _bgmAudioSource.loop = true;
        _bgmAudioSource.playOnAwake = false;
        
        _sfxAudioSource = gameObject.AddComponent<AudioSource>();
        _sfxAudioSource.loop = false;
        _sfxAudioSource.playOnAwake = false;
    }

    void Start()
    {
        LoadVolumeSettings();
        SetupEventListeners();
    }

    /// <summary>
    /// 소리 세팅 분기 함수
    /// </summary>
    void SetupEventListeners()
    {
        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
    
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }
    

    public void OnBGMVolumeChanged(float value)
    {
        BGMVolume = value;
        
        // AudioMixer 사용하는 경우
        if (_audioMixer != null)
        {
            float dbValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            _audioMixer.SetFloat("BGMVolume", dbValue);
        }
        
        // BGM AudioSource에 볼륨 적용
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.volume = value;
        }
        
        UpdateVolumeTexts();
    }

    public void OnSFXVolumeChanged(float value)
    {
        SFXVolume = value;
        
        // AudioMixer 사용하는 경우
        if (_audioMixer != null)
        {
            float dbValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            _audioMixer.SetFloat("SFXVolume", dbValue);
        }
        
        // SFX AudioSource에 볼륨 적용
        if (_sfxAudioSource != null)
        {
            _sfxAudioSource.volume = value;
        }
        
        UpdateVolumeTexts();
    }

    void UpdateVolumeTexts()
    {
        if (_bgmVolumeText != null)
            _bgmVolumeText.text = Mathf.RoundToInt(BGMVolume * 100).ToString();
        
        if (_sfxVolumeText != null)
            _sfxVolumeText.text = Mathf.RoundToInt(SFXVolume * 100).ToString();
    }

    /// <summary>
    /// BGM 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlayBGM(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < _bgmClips.Count)
        {
            var clip = _bgmClips[clipIndex].audioClip;
            if (clip != null && _bgmAudioSource != null)
            {
                _bgmAudioSource.clip = clip;
                _bgmAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// SFX 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < _sfxClips.Count)
        {
            var clip = _sfxClips[clipIndex].audioClip;
            if (clip != null && _sfxAudioSource != null)
            {
                _sfxAudioSource.PlayOneShot(clip);
            }
        }
    }
    
    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.Stop();
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGMByName(string clipName)
    {
        var clipGroup = _bgmClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && _bgmAudioSource != null)
        {
            _bgmAudioSource.clip = clipGroup.audioClip;
            _bgmAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 이름으로 SFX 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlaySFXByName(string clipName)
    {
        var clipGroup = _sfxClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && _sfxAudioSource != null)
        {
            _sfxAudioSource.PlayOneShot(clipGroup.audioClip);
        }
        else
        {
            Debug.LogWarning($"SFX 클립 '{clipName}'을 찾을 수 없습니다.");
        }
    }

    public void OnOKButtonClicked()
    {
        SaveVolumeSettings();
    }

    public void OnBackButtonClicked()
    {
        LoadVolumeSettings();
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(BGMVolumeKey, BGMVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"볼륨 설정 저장됨 - BGM: {BGMVolume:F2}, SFX: {SFXVolume:F2}");
    }

    public void LoadVolumeSettings()
    {
        BGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, 0.5f);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.5f);
        
        // UI 업데이트
        if (_bgmSlider != null)
            _bgmSlider.value = BGMVolume;
        
        if (_sfxSlider != null)
            _sfxSlider.value = SFXVolume;
        
        // 실제 볼륨 적용
        OnBGMVolumeChanged(BGMVolume);
        OnSFXVolumeChanged(SFXVolume);
        
        Debug.Log($"볼륨 설정 로드됨 - BGM: {BGMVolume:F2}, SFX: {SFXVolume:F2}");
    }

    void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        
        if (_okButton != null)
            _okButton.onClick.RemoveListener(OnOKButtonClicked);
        
        if (_backButton != null)
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}