using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/Pursuit")]

    public class PursuitSteeringBehaviour : ArriveSteeringBehaviour
    {
        protected override void GetTargetPosition()
        {
            base.GetTargetPosition();
            if (targetObject != null )
            {
                /* predict T seconds ahead */
                float prediction = distanceToTarget/MAX_VELOCITY;
                var targetVelocity = targetObject.GetComponent<CharacterController>().velocity;
                var horizontalVelocity = new Vector3(targetVelocity.x, 0, targetVelocity.z);
                targetPosition += horizontalVelocity * prediction;
            }
        }
    }
}