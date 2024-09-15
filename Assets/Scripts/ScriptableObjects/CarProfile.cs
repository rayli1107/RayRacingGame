using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "Car Profile",
        menuName = "ScriptableObjects/Car Profile")]
    public class CarProfile : ScriptableObject
    {
        [field: SerializeField]
        public string carName { get; private set; }

        //The maximum speed that the car can reach in km/h.
        [field: SerializeField]
        public int maxSpeed { get; private set; }

        //The maximum speed that the car can reach while going on reverse in km/h.
        [field: SerializeField]
        public int maxReverseSpeed { get; private set; }

        // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.
        [field: SerializeField]
        public int accelerationMultiplier { get; private set; }

        // The maximum angle that the tires can reach while rotating the steering wheel. 10-45
        [field: SerializeField]
        public int maxSteeringAngle { get; private set; }

        // How fast the steering wheel turns. 0.1 - 1
        [field: SerializeField]
        public float steeringSpeed { get; private set; }

        // The strength of the wheel brakes. (100-600)
        [field: SerializeField]
        public int brakeForce { get; private set; }

        // How fast the car decelerates when the user is not using the throttle. (1-10)
        [field: SerializeField]
        public int decelerationMultiplier { get; private set; }

        // How much grip the car loses when the user hit the handbrake. (1-10)
        [field: SerializeField]
        public int handbrakeDriftMultiplier { get; private set; }

        // This is a vector that contains the center of mass of the car. I recommend to set this value
        [field: SerializeField]
        public Vector3 bodyMassCenter { get; private set; }

        [field: SerializeField]
        public int mass { get; private set; }

        [field: SerializeField]
        public Color color { get; private set; }


        public CarProfile()
        {
            maxSpeed = 90;
            maxReverseSpeed = 45;
            accelerationMultiplier = 5;
            maxSteeringAngle = 27;
            steeringSpeed = 0.5f;
            brakeForce = 350;
            decelerationMultiplier = 2;
            handbrakeDriftMultiplier = 5;
            mass = 1500;
            color = Color.white;
        }
    }
}
