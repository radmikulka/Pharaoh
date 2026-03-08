using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Pharaoh.CoreGame
{
    public sealed class CWorkerRouteComputer
    {
        private readonly Dictionary<(Vector3, Vector3), CWorkerRoute> _cache = new();

        public CWorkerRoute GetOrCompute(Vector3 storagePos, Vector3 monumentEntryPos)
        {
            var key = (storagePos, monumentEntryPos);
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var toMonument = ComputePath(storagePos, monumentEntryPos);
            var toStorage  = ComputePath(monumentEntryPos, storagePos);
            var route = new CWorkerRoute(toMonument, toStorage);
            _cache[key] = route;
            return route;
        }

        private static Vector3[] ComputePath(Vector3 from, Vector3 to)
        {
            var path = ABPath.Construct(from, to);
            AstarPath.StartPath(path);
            path.BlockUntilCalculated();
            return path.vectorPath?.ToArray() ?? new[] { from, to };
        }

        public void ClearCache() => _cache.Clear();
    }
}
