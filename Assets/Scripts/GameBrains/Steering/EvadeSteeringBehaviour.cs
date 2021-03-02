using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
    [AddComponentMenu("Scripts/Steering/Evade")]

    public class EvadeSteeringBehaviour : PursuitSteeringBehaviour
    {
        protected override void DetermineDesiredDirection()
        {
            base.DetermineDesiredDirection();
            desiredMoveDirection *= -1;
        }
    }
}