
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

public class CarInputController
{
    public Action actionStartBraking;
    public Action actionStopBraking;
    public Func<bool> actionDecelerate;


    private bool _braking;
    public bool braking
    {
        get => _braking;
        set
        {
            if (_braking != value)
            {
                _braking = value;
                if (_braking)
                {
                    decelerating = false;
                    actionStartBraking?.Invoke();
                }
                else
                {
                    if (valueAccelerate == 0)
                    {
                        decelerating = true;
                    }
                    actionStopBraking?.Invoke();
                }
            }
        }
    }

    private float _decelerateFrequency;
    private float _nextDecelerateTime;
    private bool _decelerating;
    public bool decelerating
    {
        get => _decelerating;
        set
        {
            if (_decelerating != value)
            {
                _decelerating = value;
                _nextDecelerateTime = _decelerating ? 0 : float.MaxValue;
            }
        }
    }

    public float valueTurn;

    private float _valueAccelerate;
    public float valueAccelerate
    {
        get => _valueAccelerate;
        set
        {
            if (_valueAccelerate != value)
            {
                _valueAccelerate = value;

                if (_valueAccelerate != 0)
                {
                    decelerating = false;
                }
                else if (!braking)
                {
                    decelerating = true;
                }
            }
        }
    }


    public CarInputController(float decelerateFrequency)
    {
        _decelerateFrequency = decelerateFrequency;
        _nextDecelerateTime = float.MaxValue;
        _decelerating = false;
        _braking = false;
        valueTurn = 0;
        valueAccelerate = 0;
    }

    public void Update(float timeNow)
    {
        if (timeNow >= _nextDecelerateTime)
        {
            if (actionDecelerate.Invoke())
            {
                _nextDecelerateTime = timeNow + _decelerateFrequency;
            }
            else
            {
                decelerating = false;
            }
        }
    }
}

public class BasePrometeoCarController : MonoBehaviour
{
    [HideInInspector]
    public ScriptableObjects.CarProfile carProfile;

    [field: SerializeField]
    protected WheelController wheelFrontLeft { get; private set; }

    [field: SerializeField]
    protected WheelController wheelFrontRight { get; private set; }

    [field: SerializeField]
    protected WheelController wheelBackLeft { get; private set; }

    [field: SerializeField]
    protected WheelController wheelBackRight { get; private set; }

    [SerializeField]
    private float _decelerateFrequency = 0.1f;

    [field: SerializeField]
    public AudioSource carEngineSound { get; private set; }

    [field: SerializeField]
    public AudioSource tireScreechSound { get; private set; }

    public CarInputController carInput { get; private set; }

    //The following variable lets you to set up sounds for your car such as the car engine or tire screech sounds.
    private bool _useSounds = true;
    float initialCarEngineSoundPitch; // Used to store the initial pitch of the car engine sound.


    protected float carSpeed { get; private set; }
    protected bool isDrifting { get; private set; }
    protected bool isTractionLocked { get; private set; }

    protected Rigidbody body { get; private set; } // Stores the car's rigidbody.
    protected float steeringAxis { get; private set; } // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    protected float throttleAxis { get; private set; }  // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    protected float driftingAxis { get; private set; }
    protected float localVelocityZ { get; private set; }
    protected float localVelocityX { get; private set; }

    // Inputs

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (carProfile == null)
        {
            gameObject.SetActive(false);
        }
    }

    private List<WheelController> getWheels()
    {
        return new List<WheelController>() {
            wheelFrontLeft, wheelFrontRight, wheelBackLeft, wheelBackRight };
}

    // Start is called before the first frame update
    void Start()
    {
        // We save the initial pitch of the car engine sound.
        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }


        if (_useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!_useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }

        carInput = new CarInputController(_decelerateFrequency);
        carInput.actionStartBraking += Handbrake;
        carInput.actionStopBraking += RecoverTraction;
        carInput.actionDecelerate += DecelerateCar;
    }

    private void UpdateSpeed()
    {
        WheelCollider collider = wheelFrontLeft.wheelCollider;
        // We determine the speed of the car.
        carSpeed = 2 * Mathf.PI * collider.radius * collider.rpm * 60 / 1000;
        // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
        localVelocityX = transform.InverseTransformDirection(body.velocity).x;
        // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
        localVelocityZ = transform.InverseTransformDirection(body.velocity).z;
    }

    private void FixedUpdate()
    {
        UpdateSpeed();

        // Steering
        if (carInput.valueTurn < 0)
        {
            TurnLeft();
        }
        else if (carInput.valueTurn > 0)
        {
            TurnRight();
        }
        else if (steeringAxis != 0f)
        {
            ResetSteeringAngle();
        }

        // Accelerating
        if (carInput.valueAccelerate > 0)
        {
            GoForward();
        }
        else if (carInput.valueAccelerate < 0)
        {
            GoReverse();
        }
        else
        {
            ThrottleOff();
        }

        carInput.Update(Time.fixedTime);
    }

    // This method controls the car sounds. For example, the car engine will sound slow when the car speed is low because the
    // pitch of the sound will be at its lowest point. On the other hand, it will sound fast when the car speed is high because
    // the pitch of the sound will be the sum of the initial pitch + the car speed divided by 100f.
    // Apart from that, the tireScreechSound will play whenever the car starts drifting or losing traction.
    public void CarSounds()
    {

        if (_useSounds)
        {
            try
            {
                if (carEngineSound != null)
                {
                    float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(body.velocity.magnitude) / 25f);
                    carEngineSound.pitch = engineSoundPitch;
                }
                if ((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
                {
                    if (!tireScreechSound.isPlaying)
                    {
                        tireScreechSound.Play();
                    }
                }
                else if ((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f))
                {
                    tireScreechSound.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
        else
        {
            if (carEngineSound != null && carEngineSound.isPlaying)
            {
                carEngineSound.Stop();
            }
            if (tireScreechSound != null && tireScreechSound.isPlaying)
            {
                tireScreechSound.Stop();
            }
        }

    }

    //
    //STEERING METHODS
    //

    //The following method turns the front car wheels to the left. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnLeft()
    {
        steeringAxis = steeringAxis - (Time.deltaTime * 10f * carProfile.steeringSpeed);
        if (steeringAxis < -1f)
        {
            steeringAxis = -1f;
        }
        var steeringAngle = steeringAxis * carProfile.maxSteeringAngle;

        wheelFrontLeft.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontLeft.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
        wheelFrontRight.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontRight.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
    }

    //The following method turns the front car wheels to the right. The speed of this movement will depend on the steeringSpeed variable.
    public void TurnRight()
    {
        steeringAxis = steeringAxis + (Time.deltaTime * 10f * carProfile.steeringSpeed);
        if (steeringAxis > 1f)
        {
            steeringAxis = 1f;
        }
        var steeringAngle = steeringAxis * carProfile.maxSteeringAngle;
        wheelFrontLeft.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontLeft.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
        wheelFrontRight.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontRight.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
    }

    //The following method takes the front car wheels to their default position (rotation = 0). The speed of this movement will depend
    // on the steeringSpeed variable.
    public void ResetSteeringAngle()
    {
        if (steeringAxis < 0f)
        {
            steeringAxis = steeringAxis + (Time.deltaTime * 10f * carProfile.steeringSpeed);
        }
        else if (steeringAxis > 0f)
        {
            steeringAxis = steeringAxis - (Time.deltaTime * 10f * carProfile.steeringSpeed);
        }
        if (Mathf.Abs(wheelFrontLeft.wheelCollider.steerAngle) < 1f)
        {
            steeringAxis = 0f;
        }
        var steeringAngle = steeringAxis * carProfile.maxSteeringAngle;
        wheelFrontLeft.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontLeft.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
        wheelFrontRight.wheelCollider.steerAngle = Mathf.Lerp(
            wheelFrontRight.wheelCollider.steerAngle, steeringAngle, carProfile.steeringSpeed);
    }

    //
    //ENGINE AND BRAKING METHODS
    //

    // This method apply positive torque to the wheels in order to go forward.
    public void GoForward()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        // The following part sets the throttle power to 1 smoothly.
        throttleAxis = throttleAxis + (Time.deltaTime * 3f);
        if (throttleAxis > 1f)
        {
            throttleAxis = 1f;
        }
        //If the car is going backwards, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
        //is safe to apply positive torque to go forward.
        if (localVelocityZ < -1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.RoundToInt(carSpeed) < carProfile.maxSpeed)
            {
                Throttle(carProfile.accelerationMultiplier * 50f * throttleAxis);
            }
            else
            {
                ThrottleOff();
            }
        }
    }

    // This method apply negative torque to the wheels in order to go backwards.
    public void GoReverse()
    {
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car is losing traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        // The following part sets the throttle power to -1 smoothly.
        throttleAxis = throttleAxis - (Time.deltaTime * 3f);
        if (throttleAxis < -1f)
        {
            throttleAxis = -1f;
        }
        //If the car is still going forward, then apply brakes in order to avoid strange
        //behaviours. If the local velocity in the 'z' axis is greater than 1f, then it
        //is safe to apply negative torque to go reverse.
        if (localVelocityZ > 1f)
        {
            Brakes();
        }
        else
        {
            if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < carProfile.maxReverseSpeed)
            {
                Throttle(carProfile.accelerationMultiplier * 50f * throttleAxis);
            }
            else
            {
                ThrottleOff();
            }
        }
    }

    protected bool isStopped()
    {
        return body.velocity.magnitude < 0.25f;
    }

    // The following method decelerates the speed of the car according to the decelerationMultiplier variable, where
    // 1 is the slowest and 10 is the fastest deceleration. This method is called by the function InvokeRepeating,
    // usually every 0.1f when the user is not pressing W (throttle), S (reverse) or Space bar (handbrake).
    public bool DecelerateCar()
    {
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        // The following part resets the throttle power to 0 smoothly.
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f)
            {
                throttleAxis = throttleAxis - (Time.deltaTime * 10f);
            }
            else if (throttleAxis < 0f)
            {
                throttleAxis = throttleAxis + (Time.deltaTime * 10f);
            }
            if (Mathf.Abs(throttleAxis) < 0.15f)
            {
                throttleAxis = 0f;
            }
        }
        body.velocity = body.velocity * (1f / (1f + (0.025f * carProfile.decelerationMultiplier)));
        // Since we want to decelerate the car, we are going to remove the torque from the wheels of the car.
        ThrottleOff();
        // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely and
        // also cancel the invoke of this method.
        if (isStopped())
        {
            body.velocity = Vector3.zero;
            return false;
        }
        return true;
    }

    public void Throttle(float motorTorque)
    {
        getWheels().ForEach(w => w.Throttle(motorTorque));
    }


    //The following function set the motor torque to 0 (in case the user is not pressing either W or S).
    public void ThrottleOff()
    {
        getWheels().ForEach(w => w.ThrottleOff());
    }

    // This function applies brake torque to the wheels according to the brake force given by the user.
    public void Brakes()
    {
        getWheels().ForEach(w => w.Brake(carProfile.brakeForce));
    }

    // This function is used to make the car lose traction. By using this, the car will start drifting. The amount of traction lost
    // will depend on the handbrakeDriftMultiplier variable. If this value is small, then the car will not drift too much, but if
    // it is high, then you could make the car to feel like going on ice.
    public void Handbrake()
    {
        CancelInvoke("RecoverTraction");
        float extremumSlip = wheelFrontLeft.extremumSlip;
        // We are going to start losing traction smoothly, there is were our 'driftingAxis' variable takes
        // place. This variable will start from 0 and will reach a top value of 1, which means that the maximum
        // drifting value has been reached. It will increase smoothly by using the variable Time.deltaTime.
        driftingAxis = driftingAxis + (Time.deltaTime);
        float secureStartingPoint = driftingAxis * extremumSlip * carProfile.handbrakeDriftMultiplier;

        if (secureStartingPoint < extremumSlip)
        {
            driftingAxis = extremumSlip / (extremumSlip * carProfile.handbrakeDriftMultiplier);
        }
        if (driftingAxis > 1f)
        {
            driftingAxis = 1f;
        }
        //If the forces aplied to the rigidbody in the 'x' asis are greater than
        //3f, it means that the car lost its traction, then the car will start emitting particle systems.
        if (Mathf.Abs(localVelocityX) > 2.5f)
        {
            isDrifting = true;
        }
        else
        {
            isDrifting = false;
        }
        //If the 'driftingAxis' value is not 1f, it means that the wheels have not reach their maximum drifting
        //value, so, we are going to continue increasing the sideways friction of the wheels until driftingAxis
        // = 1f.
        if (driftingAxis < 1f)
        {
            float drift = carProfile.handbrakeDriftMultiplier * driftingAxis;
            getWheels().ForEach(w => w.Drift(drift));
        }

        // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
        // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
        isTractionLocked = true;
    }


    // This function is used to recover the traction of the car when the user has stopped using the car's handbrake.
    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = driftingAxis - (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f)
        {
            driftingAxis = 0f;
        }

        //If the 'driftingAxis' value is not 0f, it means that the wheels have not recovered their traction.
        //We are going to continue decreasing the sideways friction of the wheels until we reach the initial
        // car's grip.
        if (wheelFrontLeft.IsDrifting())
        {
            float drift = carProfile.handbrakeDriftMultiplier * driftingAxis;
            getWheels().ForEach(w => w.Drift(drift));

            Invoke("RecoverTraction", Time.deltaTime);

        }
        else if (wheelFrontLeft.IsRecovering())
        {
            getWheels().ForEach(w => w.Drift(1));
            driftingAxis = 0f;
        }
    }


}

/*
public class CarController : MonoBehaviour
{
    public VPVehicleController vehicle { get; private set; }
    public Rigidbody body { get; private set; }
    public MeshCollider vehicleCollider { get; private set; }
    private VPStandardInput _playerInput;
    private float _originalMass;

    private void Awake()
    {
        vehicle = GetComponent<VPVehicleController>();
        vehicleCollider = GetComponentInChildren<MeshCollider>(true);
        body = GetComponent<Rigidbody>();
        _originalMass = body.mass;
        _playerInput = GetComponent<VPStandardInput>();
    }

    public void SetWeight(bool heavy)
    {
        body.mass = _originalMass * (heavy ? 10 : 1);
    }

    public void EnableInput(bool enable)
    {
        _playerInput.enabled = enable;
    }
}

/*
public class CarController : MonoBehaviour
{
    [SerializeField]
    private float _motorTorque = 2000;

    [SerializeField]
    private float _brakeTorque = 2000;

    [SerializeField]
    private float _maxSpeed = 20;

    [SerializeField]
    private float _steeringRange = 30;

    [SerializeField]
    private float _steeringRangeAtMaxSpeed = 10;

    [SerializeField]
    private float _centreOfGravityOffset = -1f;

    private WheelController[] _wheelControllers;
    private Rigidbody _rigidBody;
    private PlayerInput _playerInput;
    private InputAction _actionTurn;
    private InputAction _actionAccelerate;



    private void Awake()
    {
        _wheelControllers = GetComponentsInChildren<WheelController>(true);
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.centerOfMass += Vector3.up * _centreOfGravityOffset;
        _playerInput = GetComponent<PlayerInput>();
        _actionTurn = _playerInput.actions["Turn"];
        _actionAccelerate = _playerInput.actions["Accelerate"];
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float valueTurn = _actionTurn.ReadValue<float>();
        float valueAccelerate = _actionAccelerate.ReadValue<float>();
        if (valueTurn != 0 || valueAccelerate != 0)
        {
            Debug.LogFormat("Turn {0} Accelerate {1}", valueTurn, valueAccelerate);
        }

        // Calculate current speed in relation to the forward direction of the car
        // (this returns a negative number when traveling backwards)
        float forwardSpeed = Vector3.Dot(transform.forward, _rigidBody.velocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, _maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(_motorTorque, 0, speedFactor);

        // …and to calculate how much to steer 
        // (the car steers more gently at top speed)
        float currentSteerRange = Mathf.Lerp(
            _steeringRange, _steeringRangeAtMaxSpeed, speedFactor);

        // Check whether the user input is in the same direction 
        // as the car's velocity
        //        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);
        bool isAccelerating = valueAccelerate > 0;

        foreach (WheelController wheelController in _wheelControllers)
        {
            // Apply steering to Wheel colliders that have "Steerable" enabled
            if (wheelController.steerable)
            {
                wheelController.wheelCollider.steerAngle = valueTurn * currentSteerRange;
            }

            if (isAccelerating)
            {
                // Apply torque to Wheel colliders that have "Motorized" enabled
                if (wheelController.motorized)
                {
                    wheelController.wheelCollider.motorTorque = valueAccelerate * currentMotorTorque;
                }
                wheelController.wheelCollider.brakeTorque = 0;
            }
            else
            {
                // If the user is trying to go in the opposite direction
                // apply brakes to all wheels
                wheelController.wheelCollider.brakeTorque = Mathf.Abs(valueAccelerate) * _brakeTorque;
                wheelController.wheelCollider.motorTorque = 0;
            }
        }
    }
}
*/