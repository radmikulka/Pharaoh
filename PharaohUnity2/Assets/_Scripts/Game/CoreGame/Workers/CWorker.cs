using UnityEngine;

namespace Pharaoh
{
    public sealed class CWorker
    {
        public IWorkerState  State;
        public CWorkerRoute  Route;
        public int           WaypointIndex;
        public Vector3       Position;
        public Quaternion    Rotation;
        public SMonumentTask? CurrentTask;
    }
}
