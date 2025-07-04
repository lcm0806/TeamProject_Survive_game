using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Rocket : MonoBehaviour
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
        new DialogueLine { speaker = "강민준 박사", text = "설마, 이거 귀환선 맞지? 이게 왜 여기에…", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "오디세우스", text = "안녕하세요. 여기는 귀환선 ‘오디세우스’ 입니다.", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "강민준 박사", text = "으악 깜짝이야!!!...휴우, 이 귀환선 사용할 수 있어?", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "오디세우스", text = "현재 ‘오디세우스’는 설비 결함으로 인해 무기한 점검중입니다. 해당 설비의 교체가 필요합니다.", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "강민준 박사", text = "좋아, 그 설비를 제작할 수 있는 방법을 알려줄래?", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "오디세우스", text = "전송 대상을 확인합니다. ‘박사 강민준’에게 데이터를 전송합니다…전송 완료했습니다.", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        new DialogueLine { speaker = "강민준 박사", text = "그래, 이제 곧 탈출할 수 있어!", appear = new List<string> { "Gang" }, bgm = "None", sfx = "" },
        // 계속 추가 가능
    };

    void Start()
    {
        BackgroundImage.sprite = null;
        BackgroundImage.color = new Color(0f, 0f, 0f, 0.6f);

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
