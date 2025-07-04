using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
    public List<string> appear;
    public string bgm;
    public string sfx;
    public string background;
    public Dictionary<string, string> characterImages;
}

public class PlologSystem : MonoBehaviour
{
    public GameObject Ranpy;
    public GameObject Choi;
    public GameObject Park;
    public GameObject Rachel;
    public GameObject Gang;

    public TMP_Text ScriptText;
    public TMP_Text NameText;


    public UnityEngine.UI.Image BackgroundImage;

    private int currentLine = 0;

    private Dictionary<string, GameObject> characters;

    private List<DialogueLine> dialogues = new List<DialogueLine>
    {
        new DialogueLine
        {
            speaker = "강민준 박사", text = "···이것으로 화성 탐사 XX일차 기록을 마치고 지구로 복귀하겠습니다.", appear = new List<string> { "Gang" },
            bgm = "PlologBGM", sfx = "", background = "Images/Backgrounds/Shlelter",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "강박사, V-log는 다 끝난거야?", appear = new List<string> { "Gang", "Park" }, bgm = "",
            sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "아니, 박팀장님이 시키셨으면서 어떻게 가는 날까지 놀리세요?",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "놀리는 건 아니고, 강박사가 너무 실감나게 하니까 그런거지~",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "팀장님, 이 척박하고 산소도 없는 행성에서 즐겁게 살아가기 위해서라면 어쩔 수 없습니다.",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "레이첼 박사", text = "근데 강박사님 연기가 진짜 재밌긴 재밌더라구요. 저도 그거 보면서 여기 버텼잖아요.",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "자 보세요, 은서씨도 재밌게 보셨다고 하셨잖아요~",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "연구 일지는 재미로 보는 게 아니었던 거 같은데 말이지…",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "음음, 중요한 그건 중요한 문제가 아닙니다. 중요한 건 우리가 조금이라도 이 지루한 생활에서 벗어나게 만든게 중요한거죠!",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "레이첼 박사", text = "맞아요~누구나 취미란 건 필요하기 마련이죠. 팀장님도 혼자 계실 때 소설 쓰시면서 보내셨잖아요. 글 정말 잘 쓰시던데요?",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "엇, 그거 보셨어요?! 전 계속 시도해도 문서 비밀번호를 못 풀겠던데,\n내용 저한테만 알려주시면 안돼요?",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "아니 그걸 어떻게, 내 컴퓨터는 어떻게 연거야!! 소설 쓰는건 꽁꽁 숨기고 있었는데 분명",
            appear = new List<string> { "Gang", "Park", "Rachel" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "현우초이 박사", text = "박팀장님, 셔틀에 선적 모두 완료했습니다. 이제 탑승만 하시면 바로 출발 가능합니다.",
            appear = new List<string> { "Gang", "Park", "Rachel", "Choi" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "후…좋아! 여러분, 이제 이 주황색 행성과 헤어질 시간이 왔습니다! 다들 정말 고생 많았고,\n이제 지구 위 우주정거장에서 봅시다!",
            appear = new List<string> { "Gang", "Park", "Rachel", "Choi" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "레이첼 박사", text = "팀장님도 정말 고생했어요~이제 끝났으니 잠이나 푹 자면서 가야겠다~",
            appear = new List<string> { "Gang", "Park", "Rachel", "Choi" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장",
            text =
                "강박사, 혹시 잊진 않았지? 나를 포함해서 다른 팀원들은 모두 출발하기 전에 미리 냉동수면 상태에 들어갈 예정이지만,\n강박사는 궤도에 들어가는 걸 확인한 후에 냉동수면에 들어가는 거",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "당연하죠! 제가 하고싶다고 한 건데, 화성 중력권에서 벗어날 때까지 확인만 하는 거잖아요.",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "이미 셔틀에 모든 계산이 입력되어 있으니까 걱정할 필요까진 없어. 하긴 강박사가 더 잘 알겠지만.",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "저는 화성에서도 거의 실내에서 연구했으니까 이정도는 뭐. 어차피 이제 질리도록 잘 텐데 좀만 더 깨 있어도 나쁘지 않아요.",
            appear = new List<string> { "Gang", "Park" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "박정현 팀장", text = "후훗. 그래, 알겠어. 그럼 나 먼저 자러 간다~", appear = new List<string> { "Gang", "Park" },
            bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "이 집도 이제 떠나는구나…잘 있어라 쉘터야, 다음에 올 사람도 잘 대해 달라구.",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "",
            background = "Images/Backgrounds/Whisk_4833dc3d20",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "", appear = new List<string> { }, bgm = "None", sfx = "",
            background = "Images/Backgrounds/Black",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "키오네", text = "3, 2, 1, 발사합니다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "",
            background = "Images/Backgrounds/Space_shuttle",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "키오네", text = "쿠우우웅…", appear = new List<string> { "Gang" }, bgm = "",
            sfx = "Sfx_SpaceShipEngine", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "좋아. 궤도 문제없고, 내부와 외부도 충분히 안정적이야. 이대로만 가면 눈감고도 가겠네!",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "으, 화성 벗어나려면 아직 한참 남았는데 그동안 뭐하지? 아니지, 지구에서 뭘 할지나 생각해볼까?",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "쿵", appear = new List<string> { "Gang" }, bgm = "", sfx = "Sfx_Explode",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "뭐지?! 이...이런 일은 예측하지 못했는데, 키오네, 지금 무슨 일이 일어난거야?",
            appear = new List<string> { "Gang" }, bgm = "Emergencysituation", sfx = "", background = "",
            characterImages = new Dictionary<string, string> { { "Gang", "Images/Characters/Gang_shock" } }
        },
        new DialogueLine
        {
            speaker = "키오네", text = "상황 파악중…비상 상황. 선체 내 8번 섹터에서 폭파가 감지되었습니다.", appear = new List<string> { "Gang" },
            bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "8번 섹터에서 폭파가?! 일,일단 상황을 안정시켜야 해. 당황하고 있을 시간이 없어!",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "안되겠어…여기서 해결하기엔 상황이 너무 안 좋아. 그렇다면…!", appear = new List<string> { "Gang" },
            bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "키오네, 8번 섹터로 안내해줘.", appear = new List<string> { "Gang" }, bgm = "", sfx = "",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "키오네", text = "네. 유도등을 점등하겠습니다.", appear = new List<string> { "Gang" }, bgm = "", sfx = "",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "", appear = new List<string> { }, bgm = "", sfx = "",
            background = "Images/Backgrounds/Black",
            characterImages = new Dictionary<string, string> {  }
        },



        new DialogueLine
        {
            speaker = "강민준 박사", text = "윽, 연기가…! 섹터 내에 연기가 가득 차있어. 원인이...아!", appear = new List<string> { "Gang" },
            bgm = "", sfx = "", background = "Images/Backgrounds/Broken_sector",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "파지직 - !", appear = new List<string> { "Gang" }, bgm = "", sfx = "Sfx_Electric",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "합선되고 난리가 났군…대체 왜(파지직!) 윽! 젠장, 분석할 시간이 없어!",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사",
            text = "손 쓸 수가…이대로 가다간 이 우주선은 버틸 수가 없을 꺼야.\n어떻게 하지? 반드시…반드시 해결해야 해! 아, 혹시 개폐 시스템을 이용한다면!!",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> { { "Gang", "Images/Characters/Gang_Serious" }  }
        },
        new DialogueLine
        {
            speaker = "", text = "파지직 - !", appear = new List<string> { "Gang" }, bgm = "", sfx = "Sfx_Electric",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "윽, 이런…이제 유일한 방법은 함 내 AI에게 시스템과 궤도를 수정시키고…나는 이 섹터와 함께 분리되는 것 뿐이야.",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "키오네, 당장 8번 섹터를 분리하고 지구로 복귀하는 궤도를 재계산해", appear = new List<string> { "Gang" },
            bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "키오네", text = "알겠습니다. 8번 섹터를 분리한 상태로 다시 궤도를 계산하고 작동하겠습니다. 계산 중…",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "하, 젠장. 이 방법밖에 없어. 이게 맞겠지? 나는…아냐. 지금 해야만 해!",
            appear = new List<string> { "Gang" }, bgm = "", sfx = "", background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "철컹 - !", appear = new List<string> { }, bgm = "", sfx = "Sfx_SpaceShipDoor",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },




        new DialogueLine
        {
            speaker = "강민준 박사", text = "으아아아아악!! 이대로…이대로 죽을 수는 없어! 낙하산아, 제발 작동해!!!",
            appear = new List<string> { }, bgm = "", sfx = "", background = "Images/Backgrounds/Falling_sector",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "촤아아아악!!", appear = new List<string> { }, bgm = "", sfx = "Sfx_Parachute",
            background = "", 
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "쿵", appear = new List<string> { }, bgm = "", sfx = "Sfx_Explode",
            background = "",
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "", text = "", appear = new List<string> { }, bgm = "", sfx = "",
            background = "Images/Backgrounds/Black", 
            characterImages = new Dictionary<string, string> {  }
        },


        new DialogueLine
        {
            speaker = "강민준 박사", text = "으으윽…나…살아있는 거 맞지…? 죽는 줄 알았네..", appear = new List<string> { "Gang" }, bgm = "",
            sfx = "", background = "", 
            characterImages = new Dictionary<string, string> { { "Gang", "Images/Characters/Gang" } }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "으, 빛이…아! 아하하…이런 세상에…", appear = new List<string> { "Gang" }, bgm = "", sfx = "",
            background = "", 
            characterImages = new Dictionary<string, string> {  }
        },
        new DialogueLine
        {
            speaker = "강민준 박사", text = "X됐다, 젠장", appear = new List<string> { }, bgm = "None", sfx = "",
            background = "Images/Backgrounds/Shelter-watching", 
            characterImages = new Dictionary<string, string> {  }
        },
    };

    void Start()
    {
        Ranpy.SetActive(true);
        characters = new Dictionary<string, GameObject>
        {
            { "Choi", Choi },
            { "Park", Park },
            { "Rachel", Rachel },
            { "Gang", Gang }
        };

        ShowNextLine();
    }

    // 다음 대사로 넘어가는 버튼용 함수
    public void OnClickNext()
    {
        ShowNextLine();
    }

// 전체 대사를 스킵하는 버튼용 함수
    public void OnClickSkip()
    {
        // 모든 캐릭터 숨기기
        SetCharacterVisibility(new List<string>());
        SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetShelterSceneName());
    }

    void ShowNextLine()
    {
        if (currentLine >= dialogues.Count)
        {
            ScriptText.text = "";
            NameText.text = "";


            SceneSystem.Instance.LoadSceneWithDelay(SceneSystem.Instance.GetShelterSceneName());
            return;
        }

        var line = dialogues[currentLine];
        ScriptText.text = line.text;
        NameText.text = line.speaker;

        SetCharacterVisibility(line.appear);

        // 배경 이미지 처리
        if (!string.IsNullOrEmpty(line.background))
        {
            Sprite bgSprite = Resources.Load<Sprite>(line.background);

            // if (line.background == "Images/Backgrounds/Black")
            // {
            // Ranpy.SetActive(false);
            // SkipButton.SetActive(true);
            // NextButton.SetActive(true);
            // }
            // else
            // {
            // Ranpy.SetActive(true);
            // }

            if (bgSprite != null)
            {
                BackgroundImage.sprite = bgSprite;
            }
            else
            {
                Debug.LogWarning($"배경 이미지 '{line.background}'을 찾을 수 없습니다.");
            }
        }

        // BGM 처리
        if (!string.IsNullOrEmpty(line.bgm))
        {
            if (line.bgm == "None")
            {
                AudioSystem.Instance.StopBGM();
            }
            else
            {
                AudioSystem.Instance.PlayBGMByName(line.bgm);
            }
        }

        // SFX 처리
        if (!string.IsNullOrEmpty(line.sfx))
        {
            AudioSystem.Instance.PlaySFXByName(line.sfx);
        }

        currentLine++;
    }


    void SetCharacterVisibility(List<string> activeList)
    {
        var line = dialogues[currentLine];

        foreach (var pair in characters)
        {
            string name = pair.Key;
            GameObject obj = pair.Value;

            if (activeList.Contains(name))
            {
                obj.SetActive(true);
                Debug.Log($"{name} 등장");

                // 이미지 교체 시도
                if (line.characterImages != null && line.characterImages.ContainsKey(name))
                {
                    string path = line.characterImages[name];
                    Sprite sprite = Resources.Load<Sprite>(path);

                    if (sprite != null)
                    {
                        var image = obj.GetComponent<UnityEngine.UI.Image>();
                        if (image != null)
                        {
                            image.sprite = sprite;
                            Debug.Log($"이미지 변경 성공: {name} → {path}");
                        }
                        else
                        {
                            var renderer = obj.GetComponent<SpriteRenderer>();
                            if (renderer != null)
                            {
                                renderer.sprite = sprite;
                                Debug.Log($"스프라이트 렌더러 변경 성공: {name} → {path}");
                            }
                            else
                            {
                                Debug.LogWarning($"{name}에는 Image나 SpriteRenderer가 없음");
                            }
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

}
