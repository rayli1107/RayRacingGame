
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VehiclePhysics;

public class MenuSceneUIController : BaseUIController
{
    public new static MenuSceneUIController Instance;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }


}
