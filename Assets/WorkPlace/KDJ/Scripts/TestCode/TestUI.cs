using DesignPattern;
using TMPro;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [SerializeField] private GameObject _interactUI;
    [SerializeField] private TMP_Text _itemName;

    private ObseravableProperty<bool> _isInInteract = new();

    private void Awake()
    {
        _isInInteract.Subscribe(SetUI);
    }

    private void Update()
    {
        _isInInteract.Value = SamplePlayerManager.Instance.IsInIntercation;

        if (SamplePlayerManager.Instance.InteractableItem != null)
            if (!_itemName.text.Equals(SamplePlayerManager.Instance.InteractableItem.name))
            {
                _itemName.text = SamplePlayerManager.Instance.InteractableItem.name;
            }
    }

    private void OnDestroy()
    {
        _isInInteract.Unsubscribe(SetUI);
    }

    private void SetUI(bool value)
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
}
