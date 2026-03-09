namespace Pharaoh
{
    public interface IWorkerState
    {
        IWorkerState Tick(CWorker worker, float dt, float speed);
    }
}
