using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DesignPattern;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DayScriptLine
{
    public string speaker;
    public string text;
    public List<string> appear;
    public string bgm;
    public string sfx;
    public string background;
    public Dictionary<string, string> characterImages;
}


[System.Serializable]
public class DayScriptEntry
{
    public bool isRandom = false;
    public List<DayScriptLine> lines; // 일반 대사 묶음
    public List<List<DayScriptLine>> candidateLineGroups; // 랜덤 대사 묶음

    public DayScriptEntry(DayScriptLine singleLine)
    {
        isRandom = false;
        lines = new List<DayScriptLine> { singleLine };
    }

    public DayScriptEntry(List<DayScriptLine> multipleLines)
    {
        isRandom = false;
        lines = multipleLines;
    }

    public DayScriptEntry(List<List<DayScriptLine>> randomGroups)
    {
        isRandom = true;
        candidateLineGroups = randomGroups;
    }
}

public class DayScriptSystem : Singleton<DayScriptSystem>
{
    public GameObject DayScript;
    public GameObject Choi;
    public GameObject Park;
    public GameObject Rachel;
    public GameObject Gang;

    public TMP_Text ScriptText;
    public TMP_Text NameText;

    private int currentLine = 0;
    private Dictionary<string, GameObject> characters;
    private List<DayScriptEntry> dialogues;
    
    private Dictionary<string, List<DayScriptEntry>> dayScripts;
    
    public UnityEngine.UI.Image BackgroundImage;
    
    private HashSet<int> shownDays = new HashSet<int>();
    
    private Queue<DayScriptLine> currentGroupLines = new Queue<DayScriptLine>();

    protected override void Awake()
    {
        base.Awake();
        
        dayScripts = new Dictionary<string, List<DayScriptEntry>>
        {
            { "Day1", Day1Script() },
            { "Day2", Day2Script() },
            { "Day3", Day3Script() },
            { "ShToBack1", ShToBack1() },
            { "ShToBack2", ShToBack2() },
            { "ShToBack3", ShToBack3() },
            { "ShToBack4", ShToBack4() },
            { "E_g", E_g() },
            
            { "TriggerFirstSpaceshipScene", TriggerFirstSpaceshipScene() },
            { "TriggerSpaceshipDeniedEvent", TriggerSpaceshipDeniedEvent() },
            { "StartEndingSequence", StartEndingSequence() },
            { "EndingScript", EndingScript() },
        };
    }

    void Start()
    {
        characters = new Dictionary<string, GameObject>
        {
            { "Choi", Choi },
            { "Park", Park },
            { "Rachel", Rachel },
            { "Gang", Gang }
        };
    }

    public void LoadDialogueByKey(string key)
    {
        if (dayScripts.ContainsKey(key))
        {
            dialogues = dayScripts[key];
            currentLine = 0;
            // currentGroupLines.Clear();
        }
        else
        {
            Debug.LogError($"'{key}' 키에 해당하는 대사 스크립트가 없습니다.");
        }
    }
    
     /// <summary>
    /// 현재 날짜에 맞는 대사를 불러와 보여주는 함수
    /// </summary>
    public void ShowDialoguse()
    {
        
        int currentDay = StatusSystem.Instance.GetCurrentDay();
        
        // 이미 보여준 날이면 무시
        if (shownDays.Contains(currentDay))
        {
            if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetShelterSceneName())
            {
                Debug.Log($"Day {currentDay} 스크립트는 이미 출력됨. 생략합니다.");
                return;
            }
        }

        // 첫 실행이면 대사 로드 및 출력
        string key = $"Day{currentDay}";
        LoadDialogueByKey(key);
        shownDays.Add(currentDay);

        DayScript.SetActive(true);

        if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
        
        ShowNextLine();
    }
    
    public void HideDialoguse()
    {
        DayScript.SetActive(false);
        
        if (SceneSystem.Instance.GetCurrentSceneName() == SceneSystem.Instance.GetFarmingSceneName())
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true; 
        }
    }

    // 다음 대사로 넘어가는 버튼용 함수
    public void OnClickNext()
    {
        ShowNextLine();
    }

// 전체 대사를 스킵하는 버튼용 함수
    public void OnClickSkip()
    {
        DayScript.SetActive(false);
    }

    void ShowNextLine()
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogError("ShowNextLine 호출 전에 dialogues를 초기화해야 합니다.");
            return;
        }

        if (currentGroupLines.Count > 0)
        {
            ApplyLine(currentGroupLines.Dequeue());
            return;
        }

        if (currentLine >= dialogues.Count)
        {
            ScriptText.text = "";
            NameText.text = "";
            DayScript.SetActive(false);
            return;
        }

        var entry = dialogues[currentLine];

        if (entry.isRandom)
        {
            var group = entry.candidateLineGroups[Random.Range(0, entry.candidateLineGroups.Count)];
            foreach (var line in group)
            {
                currentGroupLines.Enqueue(line);
            }
        }
        else
        {
            foreach (var line in entry.lines)
            {
                currentGroupLines.Enqueue(line);
            }
        }

        currentLine++; // 큐에 넣은 다음에 증가시켜야 중복 출력 안 됨
        ShowNextLine(); // 큐에서 첫 줄 출력
    }



    void ApplyLine(DayScriptLine line)
    {
        ScriptText.text = line.text;
        NameText.text = line.speaker;
        SetCharacterVisibility(line);
        
        // 이미지 처리
        if (!string.IsNullOrEmpty(line.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>(line.background);
            if (bgSprite != null)
            {
                BackgroundImage.sprite = bgSprite;
            }
            else Debug.LogWarning($"배경 이미지 '{line.background}'을 찾을 수 없습니다.");
        }

        // BGM 처리
        if (!string.IsNullOrEmpty(line.bgm))
        {
            if (line.bgm == "None") AudioSystem.Instance.StopBGM();
            else AudioSystem.Instance.PlayBGMByName(line.bgm);
        }

        // SFX 처리
        if (!string.IsNullOrEmpty(line.sfx))
        {
            AudioSystem.Instance.PlaySFXByName(line.sfx);
        }
    }


    void SetCharacterVisibility(DayScriptLine line)
    {
        foreach (var pair in characters)
        {
            string name = pair.Key;
            GameObject obj = pair.Value;

            if (line.appear != null && line.appear.Contains(name))
            {
                obj.SetActive(true);
                Debug.Log($"{name} 등장");

                // 캐릭터 이미지 교체 시도
                if (line.characterImages != null && line.characterImages.ContainsKey(name))
                {
                    string path = line.characterImages[name];
                    Sprite sprite = Resources.Load<Sprite>(path);

                    if (sprite != null)
                    {
                        var image = obj.GetComponent<UnityEngine.UI.Image>();
                        if (image != null)
                            image.sprite = sprite;
                        else
                        {
                            var renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                                renderer.sprite = sprite;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Sprite 경로 '{path}'를 찾을 수 없음");
                    }
                }
            }
            else
            {
                obj.SetActive(false);
                Debug.Log($"{name} 퇴장");
            }
        }
    }
    
    public void SetDialogue(List<DayScriptEntry> script)
    {
        dialogues = script; 
        currentLine = 0;
        currentGroupLines.Clear();
        ShowNextLine();
    }    
    
    /// <summary>
    /// 1일차
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> Day1Script()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "젠장, 나 혼자 화성에 떨어지다니…", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string> { { "Gang", "Images/Characters/Gang" } } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "여긴 아무것도 없는데, 어떻게…살아남아야 하지?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> {  } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "….아니지, 지금은 살았다는 것에 기뻐하자.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> { } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "여기서 어떻게 살아남아야 하지? 분명 방법이 있을꺼야!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> { } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "아, 분명 쉘터 관리용 컴퓨터는 남겨져 있을텐데? 어디…", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> { } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "쉘터를 유지하는 게 중요하단 말이지, 그걸 위해선 채광이 필수겠군.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> { } }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "일단 지형을 확인하러 나가볼까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string> { } }),
        };
    }

    /// <summary>
    /// 2일차
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> Day2Script()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "팀원들은 잘 살아서 돌아가고 있을까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "AI가 잘 안내하고 있겠지? 내가 끝까지 있었어야 했는데…", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "언젠간….팀원들을 만날 수 있을까…?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
        };
    }

    /// <summary>
    /// 3일차 랜덤 
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> Day3Script()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new List<List<DayScriptLine>>
            {
                new List<DayScriptLine> {
                    new DayScriptLine { speaker = "강민준 박사", text = "내가 화성에서 하던 작업은 엔지니어 쪽이 많았지.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "여기서 얼마나 기계가 망가지던지…화성이고 사람이고 날 너무 힘들게 했어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에 온 이유는 내가 천체학자이기도 했어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "물론 지구에서 보는 것과 별반 다르진 않지만….기분이 다르잖아?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에서 별을 관찰하는 건 언제나 기분이 좋다니까.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "아, 이 광경을 평생 보는 건 사양인데. 음, 다시 기분이 안 좋아졌어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "지구에서 누군가 화성에 거주지를 만든다 한 것 같은데", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "그때를 대비해서 지금이라도 땅을 사놓을까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에 거주한다니, 그건 정말 말도 안되는 소리야.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "흠, 생각해보니 관광용으로는 나쁘지 않을 수도. 여기 있는 김에 팬션으로 개조해볼까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에 긴 시간 거주했지만, 여긴 단점이 너무 많아.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "화성은 유튜브를 보기엔 너무 어렵단 말이지.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에 살아남기 위해 필요한 요소를 정리해볼까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "쉘터 수리하기, 도구 만들기, 끊임없이 혼잣말 하며 버틸 수 있는 멘탈 가지기…", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "주변에 아무도 없다는 건 맛있는 요리를 나 혼자 다 먹을 수 있다는 장점이 있지.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "구운 감자, 찐 감자, 으깬 감자, 오늘은…감자 스테이크를 먹어야겠군. 신난다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성의 자전주기는 24시간 37분 22초로, 지구보다 약 30분 정도 길다고 해.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "지구에서 놀 땐 30분만 더 있으면 좋겠다고 생각했는데, 그렇게 좋진 않네.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성의 극지방은 다량의 물로 이루어져 있다는 사실은 어제 알았지.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "어제 밤부터 물을 펑펑 써가며 목욕하고 싶었는데, 극지방 루트를 한번 짜봐야겠어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성의 위성은 두개로, 포보스와 데이모스가 있다고 하던데? 정말 아쉽다니까.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "만일 화성에도 바다가 있었다면 끝내주는 서핑을 탈 수 있었을 텐데 말이야.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "나보다 먼저 화성에서 조난 당한 사람이 있는데, 그 사람 이야기가 어마어마해.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "홀로 화성에서 조난당했지만 도구도 만들고 식량도 재배해 7개월 이상 생존,\n거기다 잘생기기까지 하다니…진짜 대단한 사람이야.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "만일 지구에 돌아간다면 이 경험을 게임으로 만들면 재밌지 않을까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "경일게임IT아카데미라는 곳이 있다던데, 꼭 그곳에 가서 게임 제작을 배우러 가야겠어!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "사실 화성의 모래폭풍은 그렇게 강하지 않다”…잠깐만, 그러고보니 화성인데 날씨가 너무 이상…으으윽 갑자기 머리가!!!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "아, 잘 잤다. 오늘은 빨리 광석을 채굴하러 나가볼까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성의 올림푸스 산은 지구의 에베레스트 산의 2.5배가 넘는다고 한다네.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "내가 또 집 주변 산은 다 정복했던 몸인데, 나중에 도전하러 가야지!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "화성에 사람이 살지 못하는 이유 중 하나는 화성의 내핵이 식었다는 거야.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "살아 있는 행성인 지구는 때때로 생명을 위협하지만, 죽어 있는 행성은 그 자체로 생명 그 자체를 거부한다니…자연이란 참 심오하다니까.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "내가 왜 저녁에는 쉘터 밖을 나가지 않는지 알아? 화성의 최저 온도는 -143\u00b0C~-176\u00b0C까지 내려가거든", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "지구의 오리털 점퍼 같은 건 따위로 만들어 버리는 화성을 무시하지 말라고, 에…에취!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "하…너무 심심한데 쉘터 내에 가지고 놀 수 있는 도구 같은 거 있나?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "아 참! 레이첼이 라디오 잃어버리지 않았…아, 마지막 날에 내가 찾아줬지. 괜스레 생각나네.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "사실 박팀장이랑 나는 꽤 오래전부터 알던 사이였단 말이지. 대학에서 공부할 때 도와주던 선배였어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "아직도 후회가 된다니까…술자리에서 괜히 화성 이야기를 해가지고.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
                new List<DayScriptLine>
                {
                    new DayScriptLine { speaker = "강민준 박사", text = "내가 어떤 말을 했었더라? 매일 혼자 떠드니 무슨 이야기 한지도 기억이 안나네.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() },
                    new DayScriptLine { speaker = "강민준 박사", text = "지금 머릿속에 있는 건 20개 정도라서, 똑같은 이야기를 또 말할지도 모르겠어.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }
                },
            }),
        };
    }
    
    /// <summary>
    /// 쉘터 상호작용 스크립트(복귀 시)
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> ShToBack1()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "오늘은 이만 하고 쉘터로 돌아가야지…\n내일 다시 나오자.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 쉘터 상호작용 스크립트(탐색 시작 후 100초안에 다시 상호작용시 복귀 막기)
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> ShToBack2()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "지금 들어가면 다시 탐색하러 나올 수 없다.\n조금 더 고민해볼까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 쉘터 상호작용 스크립트(탐색 시작 후 100초안에 상호작용 세번 했을 때)
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> ShToBack3()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "더 탐색하는 것이 좋긴 하지만…이 정도면 충분히 한 것 같다.\n한번만 더 생각해보자.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 쉘터 상호작용 스크립트(탐색 시작 후 100초안에 상호작용 네번 했을 때)
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> ShToBack4()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "......에라 모르겠다.\n피곤한데 뭐, 그냥 들어가자.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 이스터에그, 아이템 20개이상을 한번에 떨어뜨릴 때
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> E_g()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "예쁜 불꽃놀이다...", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    
    /// <summary>
    /// 우주선 첫 발견 스크립트
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> TriggerFirstSpaceshipScene()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "설마, 이거 귀환선 맞지? 이게 왜 여기에…", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "오디세우스", text = "안녕하세요. 여기는 귀환선 ‘오디세우스’ 입니다", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "으악 깜짝이야!!!...휴우, 이 귀환선 사용할 수 있어?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "오디세우스", text = "현재 ‘오디세우스’는 설비 결함으로 인해 무기한 점검중입니다. 해당 설비의 교체가 필요합니다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "좋아, 그 설비를 제작할 수 있는 방법을 알려줄래?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "오디세우스", text = "전송 대상을 확인합니다. ‘박사 강민준’에게 데이터를 전송합니다…전송 완료했습니다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "그래, 이제 곧 탈출할 수 있어!", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 엔딩 조건 불만족 시(엔딩 아이템 제작 전) 우주선에 다시 상호작용 했을 때
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> TriggerSpaceshipDeniedEvent()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "오디세우스", text = "'오디세우스'를 다시 검사합니다…설비 결함이 감지되었습니다. 시스템을 종료합니다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    /// <summary>
    /// 엔딩 아이템을 제작한 뒤 우주선에 다시 상호작용 했을 때
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> StartEndingSequence()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "오디세우스", text = "오디세우스를 다시 검사합니다…완료되었습니다. ‘오디세우스’ 기동 준비를 시작합니다. 탑승하시겠습니까?", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "Images/Backgrounds/TransparentWhite", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "하, 드디어…드디어 떠날 수 있다. 많은 일이 있었지만, 어찌됐든 고맙다. 화성아", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "탑승할게. 목적지는, 지구야.", appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
        };
    }
    
    
    
    /// <summary>
    /// 엔딩 
    /// </summary>
    /// <returns></returns>
    public List<DayScriptEntry> EndingScript()
    {
        return new List<DayScriptEntry>
        {
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "나는 화성을 떠나 귀한선 '오디세우스'를 타고 지구로 향했다.", appear = new List<string> { }, bgm = "", sfx = "", background = "Images/Backgrounds/Space_station", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "가던 와중 지구 위의 우주 정거장에서 온 신호를 받아 목적지를 우주 정거장으로 돌렸다.", appear = new List<string> { }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "우주 정거장에 도착하자, 그곳에는 동료들이 나를 기다리고 있었다.", appear = new List<string> {  }, bgm = "", sfx = "", background = "Images/Backgrounds/Meeting_colleague", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "나는 동료들에게 쿨하고 멋있게 등장해서 인사를 했고, 우리는 그동안 있었던 일들을 이야기했다.", appear = new List<string> { }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "돌아갈 준비를 마치고, 나는 드디어 그리워하던 지구에 도착했다.", appear = new List<string> {  }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "지구에 돌아왔을 때 나는 엄청난 스타가 되어 있었다.", appear = new List<string> { }, bgm = "", sfx = "", background = "Images/Backgrounds/Lecture", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "화성에서 꿈꾸던 삶은 아니었지만, 그때 두번째로 배운 교훈인 ‘받아드리기’로 어찌저찌 살아가고 있다.", appear = new List<string> { }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "지금은 많은 사람들에게 화성에서 배운 첫번째 교훈을 전파하려고 노력중이다.", appear = new List<string> { }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "", appear = new List<string> { }, bgm = "", sfx = "", background = "Images/Backgrounds/Black", characterImages = new Dictionary<string, string>() }),
            new DayScriptEntry(new DayScriptLine { speaker = "강민준 박사", text = "포기하지 않기", appear = new List<string> { }, bgm = "", sfx = "", background = "", characterImages = new Dictionary<string, string>() }),
        };
    }
     

}
