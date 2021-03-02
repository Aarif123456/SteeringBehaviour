using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    [AddComponentMenu("Scripts/Steering/Evade")]

    public class EvadeSteeringBehaviour : FleeSteeringBehaviour
    {
        protected override Vector3 GetTargetPosition()
        {
            targetPosition = base.GetTargetPosition();
            desiredMoveDirection = GetMoveDirection();
            distanceToTarget = GetDistanceToTarget();
            if (targetObject != null )
            {
                /* predict time required to reach target position */
                float prediction = distanceToTarget/MAX_VELOCITY;
                var targetVelocity = targetObject.GetComponent<CharacterController>().velocity;
                targetPosition += targetVelocity * prediction;
            }
            return targetPosition;
        }
    }
}