using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/Arrive")]

    public class ArriveSteeringBehaviour : SeekSteeringBehaviour
    {
        public float slowingRadius = 3f;

        protected override Vector3 DetermineDesiredDirection()
        {
            desiredMoveDirection = base.DetermineDesiredDirection();
            distanceToTarget = GetDistanceToTarget();
            if (distanceToTarget < slowingRadius) {
                /* value will always be between 0 and 1 because the above if statement */
                desiredMoveDirection *= distanceToTarget / slowingRadius;
            }
            return desiredMoveDirection;
        }
    }
}