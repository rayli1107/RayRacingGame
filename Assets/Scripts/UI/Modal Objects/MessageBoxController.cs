using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public delegate void MessageBoxHandler();

public class MessageBoxController : ModalObject
{
    [SerializeField]
    private TextMeshProUGUI _textMessage;

    public string message
    {
        get => _textMessage.text;
        set { _textMessage.text = value; }
    }

    public MessageBoxHandler messageBoxHandler;

    public void Okay()
    {
        messageBoxHandler?.Invoke();
    }
}
