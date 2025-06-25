using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

[System.Serializable]
public class AudioClipGroup
{
    public string name;
    public AudioClip audioClip;
}

public class AudioSystem : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static AudioSystem _instance;
    public static AudioSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioSystem>();
                if (_instance == null)
                {
                    GameObject audioSystemObject = new GameObject("AudioSystem");
                    _instance = audioSystemObject.AddComponent<AudioSystem>();
                    DontDestroyOnLoad(audioSystemObject);
                }
            }
            return _instance;
        }
    }

    [Header("UI 요소들")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button backButton;
    
    [Header("오디오 설정")]
    [SerializeField] private AudioMixer audioMixer;
    
    [Header("BGM 클립들")]
    [SerializeField] private List<AudioClipGroup> bgmClips = new List<AudioClipGroup>();
    
    [Header("SFX 클립들")]
    [SerializeField] private List<AudioClipGroup> sfxClips = new List<AudioClipGroup>();
    
    // AudioSource 컴포넌트들
    private AudioSource bgmAudioSource;
    private AudioSource sfxAudioSource;
    
    // 볼륨 값 저장용
    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;
    
    // PlayerPrefs 키
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (_instance == null)
        {
            _instance = this;
            
            // 부모가 있다면 루트로 이동
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            
            DontDestroyOnLoad(gameObject);
            
            // AudioSource 컴포넌트 생성
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
            
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.loop = false;
            sfxAudioSource.playOnAwake = false;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        if (okButton != null)
            okButton.onClick.AddListener(OnOKButtonClicked);
        
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public void OnBGMVolumeChanged(float value)
    {
        bgmVolume = value;
        
        // AudioMixer 사용하는 경우
        if (audioMixer != null)
        {
            float dbValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            audioMixer.SetFloat("BGMVolume", dbValue);
        }
        
        // BGM AudioSource에 볼륨 적용
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = value;
        }
        
        UpdateVolumeTexts();
    }

    public void OnSFXVolumeChanged(float value)
    {
        sfxVolume = value;
        
        // AudioMixer 사용하는 경우
        if (audioMixer != null)
        {
            float dbValue = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            audioMixer.SetFloat("SFXVolume", dbValue);
        }
        
        // SFX AudioSource에 볼륨 적용
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = value;
        }
        
        UpdateVolumeTexts();
    }

    void UpdateVolumeTexts()
    {
        if (bgmVolumeText != null)
            bgmVolumeText.text = Mathf.RoundToInt(bgmVolume * 100).ToString();
        
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolume * 100).ToString();
    }

    /// <summary>
    /// BGM 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlayBGM(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < bgmClips.Count)
        {
            var clip = bgmClips[clipIndex].audioClip;
            if (clip != null && bgmAudioSource != null)
            {
                bgmAudioSource.clip = clip;
                bgmAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// SFX 재생 (클립 인덱스)
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < sfxClips.Count)
        {
            var clip = sfxClips[clipIndex].audioClip;
            if (clip != null && sfxAudioSource != null)
            {
                sfxAudioSource.PlayOneShot(clip);
            }
        }
    }
    
    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }

    /// <summary>
    /// 이름으로 BGM 재생
    /// </summary>
    /// <param name="clipName"></param>
    public void PlayBGMByName(string clipName)
    {
        var clipGroup = bgmClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && bgmAudioSource != null)
        {
            bgmAudioSource.clip = clipGroup.audioClip;
            bgmAudioSource.Play();
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
        var clipGroup = sfxClips.Find(g => g.name == clipName);
        if (clipGroup != null && clipGroup.audioClip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clipGroup.audioClip);
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

    void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"볼륨 설정 저장됨 - BGM: {bgmVolume:F2}, SFX: {sfxVolume:F2}");
    }

    void LoadVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
        
        // UI 업데이트
        if (bgmSlider != null)
            bgmSlider.value = bgmVolume;
        
        if (sfxSlider != null)
            sfxSlider.value = sfxVolume;
        
        // 실제 볼륨 적용
        OnBGMVolumeChanged(bgmVolume);
        OnSFXVolumeChanged(sfxVolume);
        
        Debug.Log($"볼륨 설정 로드됨 - BGM: {bgmVolume:F2}, SFX: {sfxVolume:F2}");
    }

    void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        
        if (okButton != null)
            okButton.onClick.RemoveListener(OnOKButtonClicked);
        
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }
}