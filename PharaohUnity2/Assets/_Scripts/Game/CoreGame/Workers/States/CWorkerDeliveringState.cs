namespace Pharaoh
{
    public sealed class CWorkerDeliveringState : IWorkerState
    {
        private const float Duration = 0.5f;
        private float _elapsed;

        public IWorkerState Tick(CWorker worker, float dt, float speed)
        {
            _elapsed += dt;
            if (_elapsed >= Duration)
            {
                worker.WaypointIndex = 0;
                return new CWorkerWalkingToStorageState();
            }
            return this;
        }
    }
}
