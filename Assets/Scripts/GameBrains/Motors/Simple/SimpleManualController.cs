using UnityEngine;

// Add to the component menu.
namespace GameBrains.Motors.Simple
{
    [AddComponentMenu("Scripts/Simple Manual Controller")]

    // Require a SimpleBotMotor to be attached to the parent game object.
    [RequireComponent(typeof(SimpleMotor))]

    public class SimpleManualController : MonoBehaviour
    {
        public string sideAxis = "Left Stick X";
        public string forwardAxis = "Left Stick Y";

        private SimpleMotor motor;

        // After all objects are initialized, Awake is called when the script
        // is being loaded. This occurs before any Start calls.
        // Use Awake instead of the constructor for initialization.
        public void Awake()
        {
            motor = GetComponent<SimpleMotor>();

            if (motor == null)
            {
                Debug.Log("Provide a simple bot motor.");
            }
        }

        // If this behaviour is enabled, Update is called once per frame.
        public void Update()
        {
            if (motor == null || motor.isAiControlled)
            {
                return;
            }

            // Get the input vector from keyboard or analog stick
            var directionVector = new Vector3(Input.GetAxis(sideAxis), 0, Input.GetAxis(forwardAxis));

            if (directionVector != Vector3.zero)
            {
                // Get the length of the direction vector and then normalize it
                // Dividing by the length is cheaper than normalizing when we already have the length anyway
                var directionLength = directionVector.magnitude;
                directionVector = directionVector / directionLength;

                // Make sure the length is no bigger than 1
                directionLength = Mathf.Min(1, directionLength);

                // Make the input vector more sensitive towards the extremes and less sensitive in the middle
                // This makes it easier to control slow speeds when using analog sticks
                directionLength = directionLength * directionLength;

                // Multiply the normalized direction vector by the modified length
                directionVector = directionVector * directionLength;
            }

            // Apply the direction to the motor
            motor.desiredMoveDirection = transform.rotation * directionVector;
        }
    }
}