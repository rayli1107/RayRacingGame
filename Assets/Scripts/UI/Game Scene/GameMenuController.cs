using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMenuController : ModalObject
{
    [SerializeField]
    private TextMeshProUGUI _labelMessage;

    public string message
    {
        get => _labelMessage.text;
        set { _labelMessage.text = value == null ? "Menu" : value; }
    }
}
