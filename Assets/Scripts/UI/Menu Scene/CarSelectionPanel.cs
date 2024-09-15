
using ScriptableObjects;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VehiclePhysics;

public class CarSelectionPanel : ModalObject
{
    [SerializeField]
    private TextMeshProUGUI _labelCarName;

    [SerializeField]
    private TextMeshProUGUI _labelTopSpeed;

    [SerializeField]
    private TextMeshProUGUI _labelAcceleration;

    protected override void OnEnable()
    {
        base.OnEnable();
        updateCarProfile();
    }

    private void updateCarProfile()
    {
        CarProfile carProfile = GlobalDataManager.Instance.carProfiles[
            GlobalDataManager.Instance.carProfileIndex];
        _labelCarName.text = carProfile.carName;
        _labelTopSpeed.text = string.Format("{0} km/h", carProfile.maxSpeed);
        _labelAcceleration.text = string.Format("{0}x", carProfile.accelerationMultiplier);
    }

    public void onNextButton()
    {
        GlobalDataManager.Instance.carProfileIndex =
            (GlobalDataManager.Instance.carProfileIndex + 1) % GlobalDataManager.Instance.carProfiles.Length;
        updateCarProfile();
    }

    public void onPrevButton()
    {
        GlobalDataManager.Instance.carProfileIndex--;
        if (GlobalDataManager.Instance.carProfileIndex < 0)
        {
            GlobalDataManager.Instance.carProfileIndex += GlobalDataManager.Instance.carProfiles.Length; ;
        }
        updateCarProfile();
    }

    public void onStartGame()
    {
        MenuSceneUIController.Instance.ShowMessageBox(
            "Start a new game?",
            MenuSceneController.Instance.StartGame);
    }
}
