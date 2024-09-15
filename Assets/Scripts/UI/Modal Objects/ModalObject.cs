using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalObject : MonoBehaviour
{
    [field: SerializeField]
    public bool useBackground { get; private set; }

    public bool enableCancel;

    private Button[] _buttons;

    public ModalObject()
    {
        useBackground = true;
    }

    private void Awake()
    {
        enableCancel = true;
        _buttons = GetComponentsInChildren<Button>(true);
    }

    protected virtual void OnEnable()
    {
        BaseUIController.Instance.RegisterModalItem(this);
    }

    protected virtual void OnDisable()
    {
        BaseUIController.Instance.UnregisterModalItem(this);
    }

    public virtual void Cancel()
    {
        if (enableCancel)
        {
            gameObject.SetActive(false);
        }
    }

    public void EnableInput(bool enable)
    {
        foreach (Button button in _buttons)
        {
            button.enabled = enable;
        }
    }
}
