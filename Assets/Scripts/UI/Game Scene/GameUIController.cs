
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

    private float _lastUpdateTime;
    private bool _sessionDataUIEnabled;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void OnEnable()
    {
        _panelPlayerCommands.gameObject.SetActive(!GlobalDataManager.Instance.spectatorMode);
        _panelSpectateCommands.gameObject.SetActive(GlobalDataManager.Instance.spectatorMode);
        _sessionDataUIEnabled = false;
        GameController.Instance.playerCar.sessionData.updateAction += onStatUpdate;
        onStatUpdate();
        _lastUpdateTime = Time.time;
    }

    private void OnDisable()
    {
        if (_sessionDataUIEnabled)
        {
            GameController.Instance.playerCar.sessionData.updateAction -= onStatUpdate;
        }
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

    protected override void Update()
    {
        base.Update();

        if (!_sessionDataUIEnabled && GameController.Instance.playerCar.sessionData != null)
        {
            _sessionDataUIEnabled = true;
            GameController.Instance.playerCar.sessionData.updateAction += onStatUpdate;
        }

        if (_sessionDataUIEnabled)
        {
            float timeNow = Time.time;
            float startTime = GameController.Instance.playerCar.sessionData.startTime;
            if (startTime != float.MaxValue && timeNow - _lastUpdateTime > 0.01f)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeNow - startTime);
                _labelTimer.text = string.Format(
                    "{0}:{1:00}.{2:000}",
                    timeSpan.Minutes,
                    timeSpan.Seconds,
                    timeSpan.Milliseconds);
                _lastUpdateTime = timeNow;
            }

            _labelRank.text = string.Format(
                "{0} Place", _rankLabel[GameController.Instance.playerCar.sessionData.rank]);
        }
    }
    private void onStatUpdate()
    {
        _labelLapCounter.text = string.Format(
            "Lap {0} / {1}",
            GameController.Instance.playerCar.sessionData.lapCounter + 1,
            GameController.Instance.maxLapCount);
    }

    public void ShowGameOverMessageBox()
    {
        string message = string.Format(
            "Congratulations! You've finished in {0} place!",
            _rankLabel[GameController.Instance.playerCar.sessionData.rank]);
        _panelMenu.message = message;
        _panelMenu.enableCancel = false;
        _panelMenu.gameObject.SetActive(true);
    }
}
