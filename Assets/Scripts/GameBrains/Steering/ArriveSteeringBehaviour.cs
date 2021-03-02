using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/Arrive")]

    public class ArriveSteeringBehaviour : SeekSteeringBehaviour
    {
        public float slowingRadius = 5f;

        protected override void DetermineDesiredDirection()
        {
            base.DetermineDesiredDirection();
            if (distanceToTarget < slowingRadius) {
                desiredMoveDirection *= distanceToTarget / slowingRadius;
            }
        }
    }
}