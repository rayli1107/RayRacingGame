
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VehiclePhysics;

public class GameUIController : BaseUIController
{
    [SerializeField]
    private TextMeshProUGUI _labelLapCounter;

    [SerializeField]
    private TextMeshProUGUI _labelCountdown;

    [SerializeField]
    private TextMeshProUGUI _labelTimer;

    [SerializeField]
    private TextMeshProUGUI _labelRank;

    [SerializeField]
    private GameMenuController _panelMenu;

    [SerializeField]
    private RectTransform _panelPlayerCommands;

    [SerializeField]
    private RectTransform _panelSpectateCommands;


    public new static GameUIController Instance;

    private List<string> _rankLabel = new List<string>()
    {
        "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th"
    };

    private float _timerStartTime;

    private CarSessionData _carSessionData;
    public CarSessionData carSessionData
    {
        get => _carSessionData;
        set
        {
            if (_carSessionData != value)
            {
                if (_carSessionData != null)
                {
                    _carSessionData.updateAction -= onStatUpdate;
                }
                _carSessionData = value;
                if (_carSessionData != null)
                {
                    _carSessionData.updateAction += onStatUpdate;
                    onStatUpdate();
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        _carSessionData = null;

    }

    private void OnEnable()
    {
        _panelPlayerCommands.gameObject.SetActive(!GlobalDataManager.Instance.spectatorMode);
        _panelSpectateCommands.gameObject.SetActive(GlobalDataManager.Instance.spectatorMode);
    }

    public void SetCountdown(int countdown)
    {
        _labelCountdown.gameObject.SetActive(countdown > 0);
        _labelCountdown.text = countdown.ToString();
    }

    protected override void onEscapeButton()
    {
        base.onEscapeButton();
        _panelMenu.message = null;
        _panelMenu.enableCancel = true;
        _panelMenu.gameObject.SetActive(!_panelMenu.gameObject.activeInHierarchy);
    }

    public void StartTimerDisplay()
    {
        _timerStartTime = Time.time;
        InvokeRepeating(nameof(updateTimerDisplay), 0, 0.01f);
    }

    private void updateTimerDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time - _timerStartTime);
        _labelTimer.text = string.Format(
            "{0}:{1:00}.{2:000}",
            timeSpan.Minutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds);
    }

    private void onStatUpdate()
    {
        _labelLapCounter.text = string.Format(
            "Lap {0} / {1}",
            carSessionData.lapCounter + 1,
            GameController.Instance.maxLapCount);
        _labelRank.text = string.Format(
            "{0} Place", _rankLabel[carSessionData.rank]);
    }

    public void ShowGameOverMessageBoxAsPlayer()
    {
        CancelInvoke(nameof(updateTimerDisplay));
        string message = string.Format(
            "Congratulations! You've finished in {0} place!",
            _rankLabel[GameController.Instance.playerCar.sessionData.rank]);
        _panelMenu.message = message;
        _panelMenu.enableCancel = false;
        _panelMenu.gameObject.SetActive(true);
    }

    public void ShowGameOverMessageBox()
    {
        CancelInvoke(nameof(updateTimerDisplay));
        _panelMenu.message = "Game Over!";
        _panelMenu.enableCancel = false;
        _panelMenu.gameObject.SetActive(true);
    }
}
