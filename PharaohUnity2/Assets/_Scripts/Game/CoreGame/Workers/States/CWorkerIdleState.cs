namespace Pharaoh
{
    public sealed class CWorkerIdleState : IWorkerState
    {
        public IWorkerState Tick(CWorker worker, float dt, float speed)
        {
            return this;
        }
    }
}
