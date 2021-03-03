using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    
    [AddComponentMenu("Scripts/Steering/Wander")]

    public class WanderSteeringBehaviour : SeekSteeringBehaviour
    {
        /* How far circle is front-of agent */
        [SerializeField]
        public const float CIRCLE_DISTANCE = 1f;

        /* How big the circle will be */
        [SerializeField]
        public const float CIRCLE_RADIUS = 1f;

        /* The maximum amount we can change in a update */
        [SerializeField]
        public const float MOVEMENT_JITTER = 1f;

        protected override Vector3 GetTargetPosition()
        {
            /* Get the center of the circle in front of the agent */
            var circleCenter = GetComponent<CharacterController>().velocity;
            circleCenter = circleCenter.normalized;
            circleCenter *= CIRCLE_DISTANCE;
            /* Get displacement vector */
            var displacement = new Vector3(Random.Range(-MOVEMENT_JITTER, MOVEMENT_JITTER), 0, Random.Range(-MOVEMENT_JITTER, MOVEMENT_JITTER)).normalized;
            displacement *= CIRCLE_RADIUS;

            /* Now add circle center to with the displacement and add to current position */
            targetPosition = transform.position + displacement + circleCenter;
            return targetPosition;
        }
    }
}