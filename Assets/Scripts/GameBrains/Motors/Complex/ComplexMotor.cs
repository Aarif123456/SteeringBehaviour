#region Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

// Microsoft Reciprocal License (Ms-RL)
//
// This license governs use of the accompanying software. If you use the software, you accept this
// license. If you do not accept the license, do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same
// meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// copyright license to reproduce its contribution, prepare derivative works of its contribution,
// and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or
// otherwise dispose of its contribution in the software or derivative works of the contribution in
// the software.
//
// 3. Conditions and Limitations
// (A) Reciprocal Grants- For any file you distribute that contains code from the software (in
// source code or binary format), you must provide recipients the source code to that file along
// with a copy of this license, which license will govern that file. You may license other files
// that are entirely your own work and do not contain code from the software under any terms you
// choose.
// (B) No Trademark License- This license does not grant you rights to use any contributors' name,
// logo, or trademarks.
// (C) If you bring a patent claim against any contributor over patents that you claim are
// infringed by the software, your patent license from such contributor to the software ends
// automatically.
// (D) If you distribute any portion of the software, you must retain all copyright, patent,
// trademark, and attribution notices that are present in the software.
// (E) If you distribute any portion of the software in source code form, you may do so only under
// this license by including a complete copy of this license with your distribution. If you
// distribute any portion of the software in compiled or object code form, you may only do so under
// a license that complies with this license.
// (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no
// express warranties, guarantees or conditions. You may have additional consumer rights under your
// local laws which this license cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a particular purpose
// and non-infringement.

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

using System;
using System.Collections;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Motors.Complex
{
    [AddComponentMenu("Scripts/Complex Motor")]

    // Require a character controller to be attached to the parent game object.
    [RequireComponent(typeof(CharacterController))]

    public class ComplexMotor : Motor
    {
        public bool useFixedUpdate = true;
        public float pushPower = 2.0f;

        class MotorMovement
        {
            // The maximum horizontal speed when moving
            public float maxForwardSpeed = 10.0f;
            public float maxSidewaysSpeed = 10.0f;
            public float maxBackwardsSpeed = 10.0f;

            // Curve for multiplying speed based on slope (negative = downwards)
            public readonly AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

            // How fast does the character change speeds?  Higher is faster.
            public float maxGroundAcceleration = 30.0f;
            public float maxAirAcceleration = 20.0f;

            // The gravity for the bot
            public float gravity = 10.0f;
            public float maxFallSpeed = 20.0f;

            // The last collision flags returned from controller.Move
            [NonSerialized]
            public CollisionFlags collisionFlags;

            // We will keep track of the bot's current velocity,
            //[NonSerialized]
            public Vector3 velocity;

            // This keeps track of our current velocity while we're not grounded
            [NonSerialized]
            public Vector3 frameVelocity = Vector3.zero;

            [NonSerialized]
            public Vector3 hitPoint = Vector3.zero;

            [NonSerialized]
            public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
        }

        readonly MotorMovement movement = new MotorMovement();

        public enum MovementTransferOnJump
        {
            None, // The jump is not affected by velocity of floor at all.
            InitTransfer, // Jump gets its initial velocity from the floor, then gradually comes to a stop.
            PermaTransfer, // Jump gets its initial velocity from the floor, and keeps that velocity until landing.
            PermaLocked // Jump is relative to the movement of the last touched floor and will move together with that floor.
        }

        // We will contain all the jumping related variables in one helper class for clarity.
        class MotorJumping
        {
            // Can the bot jump?
            public bool enabled = true;

            // How high do we jump when a jump is requested
            public float baseHeight = 1.0f;

            // We add extraHeight units (meters) on top when continuing to request jumping
            public float extraHeight = 4.1f;

            // How much does the bot jump out perpendicular to the surface on walkable surfaces?
            // 0 means a fully vertical jump and 1 means fully perpendicular.
            public float perpAmount = 0.0f;

            // How much does the bot jump out perpendicular to the surface on too steep surfaces?
            // 0 means a fully vertical jump and 1 means fully perpendicular.
            public float steepPerpAmount = 0.5f;

            // Are we jumping? (Initiated with a jump request when not grounded yet)
            // To see if we are just in the air (initiated by jumping OR falling) see the grounded variable.
            [NonSerialized]
            public bool jumping = false;

            // Was the jump request momentary or sustained. A sustained request can result in a higher jump.
            [NonSerialized]
            public bool continueJump = false;

            // the time we jumped at (Used to determine for how long to apply extra jump power after jumping.)
            [NonSerialized]
            public float lastStartTime = 0.0f;

            [NonSerialized]
            public float lastJumpRequestTime = -100;

            [NonSerialized]
            public Vector3 jumpDirection = Vector3.up;
        }

        readonly MotorJumping jumping = new MotorJumping();

        class MotorMovingPlatform
        {
            public bool enabled = true;

            public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

            [NonSerialized]
            public Transform hitPlatform;

            [NonSerialized]
            public Transform activePlatform;

            [NonSerialized]
            public Vector3 activeLocalPoint;

            [NonSerialized]
            public Vector3 activeGlobalPoint;

            [NonSerialized]
            public Quaternion activeLocalRotation;

            [NonSerialized]
            public Quaternion activeGlobalRotation;

            [NonSerialized]
            public Matrix4x4 lastMatrix;

            [NonSerialized]
            public Vector3 platformVelocity;

            [NonSerialized]
            public bool newPlatform;
        }

        readonly MotorMovingPlatform movingPlatform = new MotorMovingPlatform();

        class MotorSliding
        {
            // Does the bot slide on too steep surfaces?
            public bool enabled = true;

            // How fast does the bot slide on steep surfaces?
            public float slidingSpeed = 15;

            // How much can the bot control the sliding direction?
            // If the value is 0.5 the bot can slide sideways with half the speed of the downwards sliding speed.
            public float sidewaysControl = 1.0f;

            // How much can the bot influence the sliding speed?
            // If the value is 0.5 the bot can speed the sliding up to 150% or slow it down to 50%.
            public float speedControl = 0.4f;
        }

        readonly MotorSliding sliding = new MotorSliding();

        [NonSerialized]
        bool grounded = true;

        [NonSerialized]
        Vector3 groundNormal = Vector3.zero;

        Vector3 lastGroundNormal = Vector3.zero;

        CharacterController controller;

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

        // If this behavior is enabled, FixedUpdate is called every fixed framerate frame.
        // FixedUpdate should be used instead of Update when dealing with Rigidbody.
        public void FixedUpdate()
        {
            if (movingPlatform.enabled)
            {
                if (movingPlatform.activePlatform != null)
                {
                    if (!movingPlatform.newPlatform)
                    {
                        ////Vector3 lastVelocity = movingPlatform.platformVelocity;

                        movingPlatform.platformVelocity =
                            (movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)
                             - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / Time.deltaTime;
                    }

                    movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
                    movingPlatform.newPlatform = false;
                }
                else
                {
                    movingPlatform.platformVelocity = Vector3.zero;
                }
            }

            if (useFixedUpdate)
            {
                UpdateFunction();
            }
        }

        // If this behavior is enabled, Update is called once per frame.
        public void Update()
        {
            if (!useFixedUpdate)
            {
                UpdateFunction();
            }
        }

        void UpdateFunction()
        {
            // We copy the actual velocity into a temporary variable that we can manipulate.
            Vector3 velocity = movement.velocity;

            // Update velocity based on input
            velocity = ApplyDesiredVelocityChange(velocity);

            // Apply gravity and jumping force
            velocity = ApplyGravityAndJumping(velocity);

            // Moving platform support
            if (MoveWithPlatform())
            {
                Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
                Vector3 moveDistance = (newGlobalPoint - movingPlatform.activeGlobalPoint);
                if (moveDistance != Vector3.zero)
                {
                    controller.Move(moveDistance);
                }

                // Support moving platform rotation as well:
                Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
                Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);

                float yRotation = rotationDiff.eulerAngles.y;
                if (yRotation != 0)
                {
                    // Prevent rotation of the local up vector
                    transform.Rotate(0, yRotation, 0);
                }
            }

            // Save lastPosition for velocity calculation.
            Vector3 lastPosition = transform.position;

            // We always want the movement to be framerate independent.  Multiplying by Time.deltaTime does this.
            Vector3 currentMovementOffset = velocity * Time.deltaTime;

            // Find out how much we need to push towards the ground to avoid loosing grounding
            // when walking down a step or over a sharp change in slope.
            float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
            if (grounded)
            {
                currentMovementOffset -= pushDownOffset * Vector3.up;
            }

            // Reset variables that will be set by collision function
            movingPlatform.hitPlatform = null;
            groundNormal = Vector3.zero;

            // Move our character!
            movement.collisionFlags = controller.Move(currentMovementOffset);

            movement.lastHitPoint = movement.hitPoint;
            lastGroundNormal = groundNormal;

            if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform)
            {
                if (movingPlatform.hitPlatform != null)
                {
                    movingPlatform.activePlatform = movingPlatform.hitPlatform;
                    movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
                    movingPlatform.newPlatform = true;
                }
            }

            // Calculate the velocity based on the current and previous position.
            // This means our velocity will only be the amount the bot actually moved as a result of collisions.
            Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
            movement.velocity = (transform.position - lastPosition) / Time.deltaTime;
            Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

            // The CharacterController can be moved in unwanted directions when colliding with things.
            // We want to prevent this from influencing the recorded velocity.
            if (oldHVelocity == Vector3.zero)
            {
                movement.velocity = new Vector3(0, movement.velocity.y, 0);
            }
            else
            {
                float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
                movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
            }

            if (movement.velocity.y < velocity.y - 0.001f)
            {
                if (movement.velocity.y < 0)
                {
                    // Something is forcing the CharacterController down faster than it should.
                    // Ignore this
                    Vector3 movementVelocity = movement.velocity;
                    movementVelocity.y = velocity.y;
                    movement.velocity = movementVelocity;
                }
                else
                {
                    // The upwards movement of the CharacterController has been blocked.
                    // This is treated like a ceiling collision - stop further jumping here.
                    jumping.continueJump = false;
                }
            }

            // We were grounded but just lost grounding
            if (grounded && !IsGroundedTest())
            {
                grounded = false;

                // Apply inertia from platform
                if (movingPlatform.enabled &&
                    (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                     movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    movement.velocity += movingPlatform.platformVelocity;
                }

                SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);

                // We pushed the bot down to ensure it would stay on the ground if there was any.
                // But there wasn't so now we cancel the downwards offset to make the fall smoother.
                transform.position += pushDownOffset * Vector3.up;
            }
            // We were not grounded but just landed on something
            else if (!grounded && IsGroundedTest())
            {
                grounded = true;
                jumping.jumping = false;
                SubtractNewPlatformVelocity();

                SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
            }

            // Moving platforms support
            if (MoveWithPlatform())
            {
                // Use the center of the lower half sphere of the capsule as reference point.
                // This works best when the bot is standing on moving tilting platforms.
                movingPlatform.activeGlobalPoint = transform.position + Vector3.up * (controller.center.y - controller.height * 0.5f + controller.radius);
                movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);

                // Support moving platform rotation as well:
                movingPlatform.activeGlobalRotation = transform.rotation;
                movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
            }
        }

        Vector3 ApplyDesiredVelocityChange(Vector3 velocity)
        {
            // Find desired velocity
            Vector3 desiredVelocity;
            if (grounded && TooSteep())
            {
                // The direction we're sliding in
                desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;

                // Find the desired movement direction projected onto the sliding direction
                Vector3 projectedMoveDirection = Vector3.Project(desiredMoveDirection, desiredVelocity);

                // Add the sliding direction, the speed control, and the sideways control vectors
                desiredVelocity =
                    desiredVelocity + projectedMoveDirection * sliding.speedControl +
                    (desiredMoveDirection - projectedMoveDirection) * sliding.sidewaysControl;

                // Multiply with the sliding speed
                desiredVelocity *= sliding.slidingSpeed;
            }
            else
            {
                desiredVelocity = GetDesiredHorizontalVelocity();
            }

            if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
            {
                desiredVelocity += movement.frameVelocity;
                desiredVelocity.y = 0;
            }

            if (grounded)
            {
                desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
            }
            else
            {
                velocity.y = 0;
            }

            // Enforce max velocity change
            float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
            Vector3 velocityChangeVector = desiredVelocity - velocity;
            if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange)
            {
                velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
            }

            // If we're in the air and don't have control, don't apply any velocity change at all.
            // If we're on the ground and don't have control we do apply it - it will correspond to friction.
            if (grounded)
            {
                velocity += velocityChangeVector;
            }

            if (grounded)
            {
                // When going uphill, the CharacterController will automatically move up by the needed amount.
                // Not moving it upwards manually prevent risk of lifting off from the ground.
                // When going downhill, DO move down manually, as gravity is not enough on steep hills.
                velocity.y = Mathf.Min(velocity.y, 0);
            }

            return velocity;
        }

        Vector3 ApplyGravityAndJumping(Vector3 velocity)
        {
            if (!jumpDesired)
            {
                jumping.continueJump = false;
                jumping.lastJumpRequestTime = -100;
            }

            if (jumpDesired && jumping.lastJumpRequestTime < 0)
            {
                jumping.lastJumpRequestTime = Time.time;
            }

            if (grounded)
            {
                velocity.y = Mathf.Min(0, velocity.y) - movement.gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;

                // When jumping up we don't apply gravity for some time when jumping higher.
                // This gives more control over jump height when jumping higher.
                if (jumping.jumping && jumping.continueJump)
                {
                    // Calculate the duration that the extra jump force should have effect.
                    // If we're still less than that duration after the jumping time, apply the force.
                    if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight))
                    {
                        // Negate the gravity we just applied, except we push in jumpDirection rather than jump upwards.
                        velocity += jumping.jumpDirection * movement.gravity * Time.deltaTime;
                    }
                }

                // Make sure we don't fall any faster than maxFallSpeed. This gives our bot a terminal velocity.
                velocity.y = Mathf.Max(velocity.y, -movement.maxFallSpeed);
            }

            if (grounded)
            {
                // Jump only if the jump button was pressed down in the last 0.2 seconds.
                // We use this check instead of checking if it's pressed down right now
                // because players will often try to jump in the exact moment when hitting the ground after a jump
                // and if they hit the button a fraction of a second too soon and no new jump happens as a consequence,
                // it's confusing and it feels like the game is buggy.
                if (jumping.enabled && (Time.time - jumping.lastJumpRequestTime < 0.2f))
                {
                    grounded = false;
                    jumping.jumping = true;
                    jumping.lastStartTime = Time.time;
                    jumping.lastJumpRequestTime = -100;
                    jumping.continueJump = true;

                    // Calculate the jumping direction
                    if (TooSteep())
                    {
                        jumping.jumpDirection = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
                    }
                    else
                    {
                        jumping.jumpDirection = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
                    }

                    // Apply the jumping force to the velocity. Cancel any vertical velocity first.
                    velocity.y = 0;
                    velocity += jumping.jumpDirection * CalculateJumpVerticalSpeed(jumping.baseHeight);

                    // Apply inertia from platform
                    if (movingPlatform.enabled &&
                        (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                         movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
                    {
                        movement.frameVelocity = movingPlatform.platformVelocity;
                        velocity += movingPlatform.platformVelocity;
                    }

                    SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    jumping.continueJump = false;
                    jumping.lastJumpRequestTime = -100;
                }
            }

            return velocity;
        }

        public void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0)
            {
                if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001f || lastGroundNormal == Vector3.zero)
                {
                    groundNormal = hit.normal;
                }
                else
                {
                    groundNormal = lastGroundNormal;
                }

                movingPlatform.hitPlatform = hit.collider.transform;
                movement.hitPoint = hit.point;
                movement.frameVelocity = Vector3.zero;
            }

            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
            {
                return;
            }

            if (hit.moveDirection.y < -0.3f)
            {
                return;
            }

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            body.velocity = pushDirection * pushPower; // TODO: apply force instead
        }

        IEnumerator SubtractNewPlatformVelocity()
        {
            // When landing, subtract the velocity of the new ground from the bot's velocity
            // since movement on ground is relative to the movement of the ground.
            if (movingPlatform.enabled &&
                (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer ||
                 movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
            )
            {
                // If we landed on a new platform, we have to wait for two FixedUpdates
                // before we know the velocity of the platform under the bot.
                if (movingPlatform.newPlatform)
                {
                    Transform platform = movingPlatform.activePlatform;
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    if (grounded && platform == movingPlatform.activePlatform)
                    {
                        yield return 1;
                    }
                }
                movement.velocity -= movingPlatform.platformVelocity;
            }
        }

        bool MoveWithPlatform()
        {
            return (movingPlatform.enabled &&
                    (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked) &&
                    movingPlatform.activePlatform != null);
        }

        Vector3 GetDesiredHorizontalVelocity()
        {
            // Find desired velocity
            Vector3 desiredLocalDirection = transform.InverseTransformDirection(desiredMoveDirection);
            float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);

            if (grounded)
            {
                // Modify max speed on slopes based on slope speed multiplier curve
                float movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y) * Mathf.Rad2Deg;
                maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
            }

            return transform.TransformDirection(desiredLocalDirection * maxSpeed * DesiredSpeedFactor);
        }

        Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
        {
            Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
            return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
        }

        bool IsGroundedTest()
        {
            return (groundNormal.y > 0.01f);
        }

        float GetMaxAcceleration(bool grounded)
        {
            // Maximum acceleration on ground and in air
            if (grounded)
            {
                return movement.maxGroundAcceleration;
            }
            else
            {
                return movement.maxAirAcceleration;
            }
        }

        float CalculateJumpVerticalSpeed(float targetJumpHeight)
        {
            // From the jump height and gravity we deduce the upwards speed
            // for the bot to reach at the apex.
            return Mathf.Sqrt(2 * targetJumpHeight * movement.gravity);
        }

        bool TooSteep()
        {
            return groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
        }

        // Project a direction onto elliptical quater segments based on forward, sideways, and backwards speed.
        // The function returns the length of the resulting vector.
        float MaxSpeedInDirection(Vector3 desiredMovementDirection)
        {
            if (desiredMovementDirection == Vector3.zero)
            {
                return 0;
            }

            float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
            Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
            float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * movement.maxSidewaysSpeed;
            return length;
        }

        bool IsJumping()
        {
            return jumping.jumping;
        }

        bool IsSliding()
        {
            return (grounded && sliding.enabled && TooSteep());
        }

        bool IsTouchingCeiling()
        {
            return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;
        }

        bool IsGrounded()
        {
            return grounded;
        }

        Vector3 GetDirection()
        {
            return desiredMoveDirection;
        }

        void SetVelocity(Vector3 velocity)
        {
            grounded = false;
            movement.velocity = velocity;
            movement.frameVelocity = Vector3.zero;
            SendMessage("OnExternalVelocity");
        }
    }
}