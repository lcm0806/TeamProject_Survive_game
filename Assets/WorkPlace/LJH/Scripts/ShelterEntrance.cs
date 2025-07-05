using UnityEngine;

public class ShelterEntrance : Structure
{
    [Tooltip("SceneSystem�� ��ϵ� �����͡� ������ ���ư��ϴ�.")]
    [SerializeField] private ResultUI _resultUI;

    private int _interactCount = 0;
    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;
    }

    private void LateUpdate()
    {
        // MenuSystem �ν��Ͻ� �Ǵ� PauseMenu�� null�̸� ���� ����
        if (MenuSystem.Instance == null || MenuSystem.Instance.PauseMenu == null)
            return;

        // DayScriptSystem �ν��Ͻ� �Ǵ� DayScript�� null�̸� ���� ����
        if (DayScriptSystem.Instance == null || DayScriptSystem.Instance.DayScript == null)
            return;

        if (!MenuSystem.Instance.PauseMenu.activeSelf)
        {
            if (!DayScriptSystem.Instance.DayScript.activeSelf && Cursor.lockState != CursorLockMode.Locked && !SampleUIManager.Instance.inventoryPanel.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
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
