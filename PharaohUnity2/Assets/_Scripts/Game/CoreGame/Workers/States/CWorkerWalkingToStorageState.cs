namespace Pharaoh
{
    public sealed class CWorkerWalkingToStorageState : IWorkerState
    {
        public IWorkerState Tick(CWorker worker, float dt, float speed)
        {
            CWorkerMovement.MoveAlongPath(worker, worker.Route.ToStorage, dt, speed);
            if (worker.WaypointIndex >= worker.Route.ToStorage.Length)
            {
                worker.WaypointIndex = 0;
                return new CWorkerWalkingToMonumentState();
            }
            return this;
        }
    }
}
