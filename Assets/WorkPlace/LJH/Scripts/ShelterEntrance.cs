using UnityEngine;

public class ShelterEntrance : Structure
{
    [Tooltip("SceneSystem에 등록된 ‘쉘터’ 씬으로 돌아갑니다.")]
    [SerializeField] private ResultUI _resultUI;

    private int _interactCount = 0;
    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (MenuSystem.Instance.PauseMenu.activeSelf == false && SampleUIManager.Instance.inventoryPanel.activeSelf == false)
            if (DayScriptSystem.Instance.DayScript.activeSelf == false && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
    }

    public override void Interact()
    {
        if (_timer < 100)
        {
            _interactCount++;

            DayScriptSystem.Instance.ShowDialoguse();

            switch (_interactCount)
            {
                case 1:
                    Cursor.lockState = CursorLockMode.None;
                    DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.ShToBack1());
                    break;
                case 2:
                    Cursor.lockState = CursorLockMode.None;
                    DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.ShToBack2());
                    break;
                case 3:
                    Cursor.lockState = CursorLockMode.None;
                    DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.ShToBack3());
                    break;
                case 4:
                    Cursor.lockState = CursorLockMode.None;
                    DayScriptSystem.Instance.SetDialogue(DayScriptSystem.Instance.ShToBack4());
                    break;
                default:
                    _resultUI.OnResultUI();
                    break;
            }
        }
        else
        {
            _resultUI.OnResultUI();
        }
    }
}
