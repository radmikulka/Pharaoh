using UnityEngine;

namespace Pharaoh
{
    public sealed class CWorkerPath : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;

        private Vector3[] _toMonument;
        private Vector3[] _toStorage;

        public void Bake()
        {
            int count = _waypoints.Length;

            _toMonument = new Vector3[count];
            for (int i = 0; i < count; i++)
                _toMonument[i] = _waypoints[i].position;

            _toStorage = new Vector3[count];
            for (int i = 0; i < count; i++)
                _toStorage[i] = _toMonument[count - 1 - i];
        }

        public CWorkerRoute GetRoute() => new(_toMonument, _toStorage);
    }
}
