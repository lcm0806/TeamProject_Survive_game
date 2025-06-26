using DesignPattern;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    [SerializeField] private GameObject _interactUI;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _air;
    [SerializeField] private TMP_Text _electric;
    [SerializeField] private Image _interactDelay;

    private ObseravableProperty<bool> _isInInteract = new();

    private void Start()
    {
        _isInInteract.Subscribe(SetInteractUI);
        PlayerManager.Instance.AirGauge.Subscribe(SetTextUI);
        PlayerManager.Instance.ElecticGauge.Subscribe(SetTextUI);
        SetTextUI(1);
    }

    private void Update()
    {
        _isInInteract.Value = PlayerManager.Instance.IsInIntercation;

        if (PlayerManager.Instance.InteractableItem != null)
        {
            if (!_itemName.text.Equals(PlayerManager.Instance.InteractableItem.name))
            {
                _itemName.text = PlayerManager.Instance.InteractableItem.name;
            }
        }
        else if (PlayerManager.Instance.InteractableStructure != null)
        {
            if (!_itemName.text.Equals(PlayerManager.Instance.InteractableStructure.name))
            {
                _itemName.text = PlayerManager.Instance.InteractableStructure.name;
            }
        }

        SetInteractDelay(PlayerManager.Instance.InteractDelay);
    }

    private void OnDestroy()
    {
        _isInInteract.Unsubscribe(SetInteractUI);
        PlayerManager.Instance.AirGauge.Unsubscribe(SetTextUI);
        PlayerManager.Instance.ElecticGauge.Unsubscribe(SetTextUI);
    }

    private void SetInteractUI(bool value)
    {
        if (_isInInteract.Value)
        {
            _interactUI.SetActive(true);

        }
        else
        {
            _interactUI.SetActive(false);
        }
    }
    private void SetTextUI(float value)
    {
        _air.text = "Air : " + PlayerManager.Instance.AirGauge.Value.ToString("F1");
        _electric.text = "Electric : " + PlayerManager.Instance.ElecticGauge.Value.ToString("F1");
    }

    private void SetInteractDelay(float value)
    {
        _interactDelay.fillAmount = value;
    }
}
