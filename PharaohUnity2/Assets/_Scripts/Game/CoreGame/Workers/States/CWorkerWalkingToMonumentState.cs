namespace Pharaoh
{
    public sealed class CWorkerWalkingToMonumentState : IWorkerState
    {
        public IWorkerState Tick(CWorker worker, float dt, float speed)
        {
            CWorkerMovement.MoveAlongPath(worker, worker.Route.ToMonument, dt, speed);
            if (worker.WaypointIndex >= worker.Route.ToMonument.Length)
                return new CWorkerDeliveringState();
            return this;
        }
    }
}
