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
    
    private List<DialogueLine> dialogues = new List<DialogueLine>
    {
        new DialogueLine { speaker = "강민준 박사", text = "…이것으로 화성 탐사 XX일차 기록을 마치고 지구로 복귀하겠습니다.", appear = new List<string> { "Gang" }, bgm = "MainBGM", sfx = "", background = "Images/Backgrounds/Whisk_80e7bff22e" },
        new DialogueLine { speaker = "박정현 팀장", text = "강박사, V-log는 다 끝난거야?", appear = new List<string> { "Gang", "Park" }, bgm = "None", sfx = "", background = "Images/Backgrounds/Whisk_4833dc3d20" },
        new DialogueLine { speaker = "강민준 박사", text = "젠장…폭발이?!", appear = new List<string> { "Gang" }, bgm = "", sfx = "Sfx_Explode", background = "Images/Backgrounds/Whisk_80e7bff22e" },
        new DialogueLine { speaker = "강민준 박사", text = "으아아아악!!", appear = new List<string> { "Gang" }, bgm = "None", sfx = "Sfx_SpaceShipDoor", background = "Images/Backgrounds/Whisk_2e4947d9d1" },
        // 계속 추가 가능
    };

    void Start()
    {
        Ranpy.SetActive(true);
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
        Choi.SetActive(false);
        Park.SetActive(false);
        Rachel.SetActive(false);
        Gang.SetActive(false);

        foreach (string name in activeList)
        {
            switch (name)
            {
                case "Choi": Choi.SetActive(true); break;
                case "Park": Park.SetActive(true); break;
                case "Rachel": Rachel.SetActive(true); break;
                case "Gang": Gang.SetActive(true); break;
            }
        }
        
    }
}
