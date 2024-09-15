
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VehiclePhysics;

public class BaseUIController : MonoBehaviour
{
    [SerializeField]
    private RectTransform _panelModalBackground;

    [SerializeField]
    private MessageBoxController _messageBoxController;

    public static BaseUIController Instance;

    protected List<ModalObject> modalObjects { get; private set; }
    protected PlayerInput playerInput { get; private set; }
    protected InputAction actionPause { get; private set; }
    private float _timeScale;



    protected virtual void Awake()
    {
        Instance = this;
        modalObjects = new List<ModalObject>();
        playerInput = GetComponent<PlayerInput>();
        actionPause = playerInput.actions["Pause"];
    }

    protected virtual void onEscapeButton()
    {

    }

    protected virtual void Update()
    {
        if (actionPause.triggered)
        {
            if (modalObjects.Count == 0)
            {
                onEscapeButton();
            }
            else
            {
                modalObjects[modalObjects.Count - 1].Cancel();
            }
        }
    }

    public void RegisterModalItem(ModalObject modalObject)
    {
        if (modalObjects.Count == 0)
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        else
        {
            modalObjects[modalObjects.Count - 1].EnableInput(false);
        }
        modalObjects.Add(modalObject);
        _panelModalBackground.gameObject.SetActive(
            modalObjects.Exists(m => m.useBackground));
    }

    public void UnregisterModalItem(ModalObject modalObject)
    {
        modalObjects.Remove(modalObject);
//        Debug.LogFormat("UnregisterModalItem {0} {1} {2}", gameObject.name, modalObject.gameObject.name, modalObjects.Count);
        _panelModalBackground.gameObject.SetActive(
            modalObjects.Exists(m => m.useBackground));
        if (modalObjects.Count == 0)
        {
            Time.timeScale = _timeScale;
        }
        else
        {
            modalObjects[modalObjects.Count - 1].EnableInput(true);
        }
    }

    public void ShowMessageBox(
        string message,
        MessageBoxHandler handler = null)
    {
        _messageBoxController.message = message;
        _messageBoxController.messageBoxHandler = handler;
        _messageBoxController.gameObject.SetActive(true);
    }
}
