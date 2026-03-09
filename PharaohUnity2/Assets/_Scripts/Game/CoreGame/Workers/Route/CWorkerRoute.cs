using UnityEngine;

namespace Pharaoh
{
    public sealed class CWorkerRoute
    {
        public readonly Vector3[] ToMonument;
        public readonly Vector3[] ToStorage;

        public CWorkerRoute(Vector3[] toMonument, Vector3[] toStorage)
        {
            ToMonument = toMonument;
            ToStorage  = toStorage;
        }
    }
}
