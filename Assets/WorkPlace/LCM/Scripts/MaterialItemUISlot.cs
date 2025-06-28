using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialItemUISlot : MonoBehaviour
{
    [SerializeField] public Image materialIcon; // 인스펙터에서 할당
    [SerializeField] public TextMeshProUGUI materialName; // 인스펙터에서 할당
    [SerializeField] public TextMeshProUGUI materialQuantity; // 인스펙터에서 할당

    // 나중에 UI를 업데이트하는 메서드를 추가할 수도 있습니다.
    public void SetUI(Sprite icon, string name, string quantityText, Color quantityColor)
    {
        if (materialIcon != null) materialIcon.sprite = icon;
        if (materialName != null) materialName.text = name;
        if (materialQuantity != null)
        {
            materialQuantity.text = quantityText;
            materialQuantity.color = quantityColor;
        }
    }
}
