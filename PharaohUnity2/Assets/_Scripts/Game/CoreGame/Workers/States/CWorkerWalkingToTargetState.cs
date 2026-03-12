using UnityEngine;

namespace Pharaoh
{
    public sealed class CWorkerWalkingToTargetState : IWorkerState
    {
        private readonly Vector3[] _waypoints;

        public CWorkerWalkingToTargetState(Vector3 target)
        {
            _waypoints = new[] { target };
        }

        public IWorkerState Tick(CWorker worker, float dt, float speed)
        {
            CWorkerMovement.MoveAlongPath(worker, _waypoints, dt, speed);
            if (worker.WaypointIndex >= _waypoints.Length)
            {
                return new CWorkerDeliveringState();
            }
            return this;
        }
    }
}
