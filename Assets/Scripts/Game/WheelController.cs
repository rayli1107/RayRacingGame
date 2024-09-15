
using UnityEngine;
using UnityEngine.InputSystem;

public class WheelController : MonoBehaviour
{
    [field: SerializeField]
    public WheelCollider wheelCollider { get; private set; }

/*
 * [field: SerializeField]
    public bool steerable { get; private set; }

    [field: SerializeField]
    public bool motorized { get; private set; }
*/
    private WheelFrictionCurve _wheelFriction;
    public float extremumSlip { get; private set; }

    private void Awake()
    {
        extremumSlip = wheelCollider.sidewaysFriction.extremumSlip;

        _wheelFriction = new WheelFrictionCurve();
        _wheelFriction.extremumSlip = wheelCollider.sidewaysFriction.extremumSlip;
        _wheelFriction.extremumValue = wheelCollider.sidewaysFriction.extremumValue;
        _wheelFriction.asymptoteSlip = wheelCollider.sidewaysFriction.asymptoteSlip;
        _wheelFriction.asymptoteValue = wheelCollider.sidewaysFriction.asymptoteValue;
        _wheelFriction.stiffness = wheelCollider.sidewaysFriction.stiffness;
    }

    void Update()
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        transform.position = position;
        transform.rotation = rotation;
    }

    public void Throttle(float motorTorque)
    {
        wheelCollider.brakeTorque = 0;
        wheelCollider.motorTorque = motorTorque;
    }


    //The following function set the motor torque to 0 (in case the user is not pressing either W or S).
    public void ThrottleOff()
    {
        wheelCollider.motorTorque = 0;
    }

    // This function applies brake torque to the wheels according to the brake force given by the user.
    public void Brake(float brakeForce)
    {
        wheelCollider.brakeTorque = brakeForce;
    }

    public void Drift(float drift)
    {
        _wheelFriction.extremumSlip = extremumSlip * drift;
        wheelCollider.sidewaysFriction = _wheelFriction;
    }

    public bool IsDrifting()
    {
        return _wheelFriction.extremumSlip > extremumSlip;
    }

    public bool IsRecovering()
    {
        return _wheelFriction.extremumSlip < extremumSlip;
    }
}
