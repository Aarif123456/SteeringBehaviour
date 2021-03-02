using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/Pursuit")]

    public class PursuitSteeringBehaviour : ArriveSteeringBehaviour
    {
        /* predict T seconds ahead */
        public float prediction = 5f;
        protected override void GetTargetPosition()
        {
            if (targetObject != null )
            {
                var targetVelocity = targetObject.GetComponent<CharacterController>().velocity;
                var horizontalVelocity = new Vector3(targetVelocity.x, 0, targetVelocity.z);
                var targetTransform = targetObject.transform;
                targetPosition = targetTransform.position + horizontalVelocity * prediction;
            }
        }
    }
}