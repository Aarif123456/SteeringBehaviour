using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    public enum Deceleration{
        Slow=3,
        Normal=2,
        Fast=1
    }

    [AddComponentMenu("Scripts/Steering/Arrive")]
   
    // Require a motor to be attached to the parent game object.
    [RequireComponent(typeof(Motor))]

    public class ArriveSteeringBehaviour : SteeringBehaviour
    {
        public const float DECELERATION_TWEAKER = 0.3f;
        public Deceleration decelerationMode = Deceleration.Fast;
        protected override void DetermineDesiredDirection()
        {
            if (motor != null && motor.isAiControlled)
            {
                if (targetObject != null)
                {
                    targetPosition = targetObject.transform.position;
                }

                // vector from current to target position.
                desiredMoveDirection = targetPosition - transform.position;
                desiredMoveDirection.y = 0;

                // Get the length of the direction vector which is the distance to the target.
                distanceToTarget = desiredMoveDirection.magnitude;

                // Reduce speed based on how close we are to target 
                desiredMoveDirection *= (distanceToTarget/((float)decelerationMode * DECELERATION_TWEAKER));

                // if not there yet ...
                if (distanceToTarget > satisfactionRadius)
                {
                    // Dividing by the length is cheaper than normalizing when we already have the length anyway
                    desiredMoveDirection /= distanceToTarget;

                    // Multiply the normalized direction vector by the distance capped at 1.
                    desiredMoveDirection *= Mathf.Min(1, distanceToTarget);
                }
                else
                {
                    // we're there (close enough). Stop.
                    desiredMoveDirection = Vector3.zero;
                }
            }
            else
            {
                desiredMoveDirection = Vector3.zero;
            }
        }
    }
}