
/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarSessionData
{
    public Action updateAction;

    private int _lapCounter;
    public int lapCounter
    {
        get => _lapCounter;
        set
        {
            if (_lapCounter != value)
            {
                _lapCounter = value;
                updateAction?.Invoke();
            }
        }
    }

    private int _checkpointCounter;
    public int checkpointCounter
    {
        get => _checkpointCounter;
        set
        {
            if (_checkpointCounter != value)
            {
                _checkpointCounter = value;
                updateAction?.Invoke();
            }
        }
    }

    public float startTime;
    public float lastCheckpointTime;

    private int _rank;
    public int rank
    {
        get => _rank;
        set
        {
            if (_rank != value)
            {
                _rank = value;
                updateAction?.Invoke();
            }
        }
    }
    public CarSessionData()
    {
        lapCounter = 0;
        checkpointCounter = -1;
        startTime = Time.time;
    }
}



public class CarController : BasePrometeoCarController
{
    [SerializeField]
    private float _accelerateAngleThreshold = 2f;

    [SerializeField]
    private float _accelerateTurnAngleThreshold = 20f;

    [SerializeField]
    private float _pathingUpdateFrequency = 0.1f;

    [field: SerializeField]
    public MeshRenderer bodyMeshRenderer { get; private set; }

    public CarSessionData sessionData { get; private set; }
    public bool isPlayer;
    public bool isAI;

    private bool _hasFocus;
    public bool hasFocus
    {
        get => _hasFocus;
        set {
            if (_hasFocus != value)
            {
                _hasFocus = value;
                if (carEngineSound != null)
                {
                    carEngineSound.enabled = _hasFocus;
                }
                if (tireScreechSound != null)
                {
                    tireScreechSound.enabled = _hasFocus;
                }
            }
        }
    }

    private PlayerInput _playerInput;
    private InputAction _actionTurn;
    private InputAction _actionAccelerate;
    private InputAction _actionBrake;

    // AI
    private float _lastUpdateTime;
    private Vector3 _targetPosition;
    private bool _turnPreviousUpdate;




    protected override void Awake()
    {
        base.Awake();
        sessionData = new CarSessionData();
        _playerInput = GetComponent<PlayerInput>();
        _actionTurn = _playerInput.actions["Turn"];
        _actionAccelerate = _playerInput.actions["Accelerate"];
        _actionBrake = _playerInput.actions["Brake"];
    }

    private void OnEnable()
    {
        _turnPreviousUpdate = false;
        if (isAI)
        {
            InvokeRepeating(nameof(updateAIPathing), 0f, _pathingUpdateFrequency);
        }
    }

    private void Update()
    {
        float valueTurn = _actionTurn.ReadValue<float>();
        float valueAccelerate = _actionAccelerate.ReadValue<float>();
        bool valueBrake = _actionBrake.IsPressed();

        if (isPlayer)
        {
            if (!isAI || valueTurn != 0)
            {
                carInput.valueTurn = valueTurn;
            }
            if (!isAI || valueAccelerate != 0)
            {
                carInput.valueAccelerate = valueAccelerate;
            }
            if (!isAI || valueBrake)
            {
                carInput.braking = valueBrake;
            }

            bool hasInput = valueTurn != 0 || valueAccelerate != 0 || valueBrake;
            string invokeName = nameof(updateAIPathing);
            if (isAI)
            {
                if (hasInput)
                {
                    Debug.Log("Canceling AI");
                    CancelInvoke(invokeName);
                }
                else if (!IsInvoking(invokeName))
                {
                    Debug.Log("Starting AI");
                    InvokeRepeating(invokeName, 1f, _pathingUpdateFrequency);
                }
            }
        }
    }

    private void updateAIPathing()
    {
        Vector3 carForward = transform.forward;
        carForward.y = 0;
        Vector3 targetDirection = _targetPosition - transform.position;
        targetDirection.y = 0;
        float angle = Vector3.SignedAngle(carForward, targetDirection, Vector3.up);

/*        Debug.LogFormat(
            "Car {0} Target {1} Car Forward {2} TargetDirection {3} Angle {4}",
            transform.position,
            _targetPosition,
            carForward,
            targetDirection,
            angle);
*/
        if (Mathf.Abs(angle) <= _accelerateAngleThreshold)
        {
//            Debug.LogFormat("Angle {0}, forward, carSpeed {1}/{2}", angle, carSpeed, carProfile.maxSpeed);
            carInput.valueAccelerate = 1;
            carInput.valueTurn = 0;
        }
        else if (angle > 0)
        {
            if (angle < _accelerateTurnAngleThreshold)
            {
//                Debug.LogFormat("Angle {0}, forward right, carSpeed {1}/{2}", angle, carSpeed, carProfile.maxSpeed);
                carInput.valueAccelerate = carSpeed / carProfile.maxSpeed > 0.7f ? 0 : 1;
                carInput.valueTurn = _turnPreviousUpdate ? 0 : 1;
            }
            else
            {
//                Debug.LogFormat("Angle {0}, backwards left, carSpeed {1}/{2}", angle, carSpeed, carProfile.maxSpeed);
                carInput.valueAccelerate = -1;
                carInput.valueTurn = -1;
            }
        }
        else
        {
            if (angle > -1 * _accelerateTurnAngleThreshold)
            {
//                Debug.LogFormat("Angle {0}, forward left, carSpeed {1}/{2}", angle, carSpeed, carProfile.maxSpeed);
                carInput.valueAccelerate = carSpeed / carProfile.maxSpeed > 0.7f ? 0 : 1;
                carInput.valueTurn = _turnPreviousUpdate ? 0 : -1;
            }
            else
            {
//                Debug.LogFormat("Angle {0}, backwards right, carSpeed {1}/{2}", angle, carSpeed, carProfile.maxSpeed);
                carInput.valueAccelerate = -1;
                carInput.valueTurn = 1;

            }
        }
        _turnPreviousUpdate = carInput.valueTurn != 0;
    }

    public void NextCheckpoint(CheckpointController checkpoint, Vector3 prevContactPoint)
    {
        _targetPosition = checkpoint.AddTarget(transform.position, prevContactPoint);
        /*
        Vector3 carForward = transform.forward;
        carForward.y = 0;
        Vector3 targetDirection = targetPosition - transform.position;
        targetDirection.y = 0;
        float angle = Vector3.SignedAngle(carForward, targetDirection, Vector3.up);
        Debug.LogFormat("Angle {0}", angle);
        */
    }

    public void NextCheckpoint(CheckpointController checkpoint)
    {
        _targetPosition = checkpoint.AddTarget(transform.position);
    }
}