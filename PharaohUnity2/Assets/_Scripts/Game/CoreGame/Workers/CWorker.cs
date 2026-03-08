using UnityEngine;

namespace Pharaoh.CoreGame
{
    public sealed class CWorker
    {
        public EWorkerState State;
        public CWorkerRoute  Route;
        public int           WaypointIndex;
        public Vector3       Position;
        public float         PauseTimer;
        public int           ViewIndex;
    }
}
