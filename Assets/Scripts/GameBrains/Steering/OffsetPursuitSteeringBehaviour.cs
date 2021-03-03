using GameBrains.Motors;
using UnityEngine;

namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/OffsetPursuit")]

    public class OffsetPursuitSteeringBehaviour : ArriveSteeringBehaviour
    {
        [SerializeField]
        protected Vector3 offsetVector = new Vector3(0f, 0f, -5);
        protected override Vector3 GetTargetPosition()
        {
            targetPosition = base.GetTargetPosition();
            if (targetObject != null )
            {
                targetPosition += offsetVector;
            }
            return targetPosition;
        }
    }
}