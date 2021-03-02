using UnityEngine;

// Add to the component menu.
namespace GameBrains.Motors.Simple
{
    [AddComponentMenu("Scripts/Simple Motor")]

    // Require a character controller to be attached to the parent game object.
    [RequireComponent(typeof(CharacterController))]
    public class SimpleMotor : Motor
    {
        public float maxSpeed = 10.0f;
        public float gravity = 10.0f;

        CharacterController controller;
        Vector3 moveDirection = Vector3.zero;

        // After all objects are initialized, Awake is called when the script
        // is being loaded. This occurs before any Start calls.
        // Use Awake instead of the constructor for initialization.
        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<CharacterController>();

            if (controller == null)
            {
                Debug.Log("Please assign a CharacterController component.");
            }
        }

        // If this behavior is enabled, Update is called once per frame.
        public void Update()
        {
            if (controller.isGrounded)
            {
                moveDirection = desiredMoveDirection * (maxSpeed * DesiredSpeedFactor);
            }

            moveDirection.y -= gravity * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}