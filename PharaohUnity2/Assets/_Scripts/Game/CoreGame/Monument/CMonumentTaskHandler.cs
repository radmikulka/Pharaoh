using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh
{
    public sealed class CMonumentTaskHandler
    {
        private Vector3[] _cellPositions;
        private CWorkerRoute[] _cellRoutes;
        private int _nextIndex;
        private int _completedCount;

        public bool IsComplete => _cellPositions != null && _completedCount >= _cellPositions.Length;

        public void Initialize(CMonumentInstance monument)
        {
            var positions = new List<Vector3>();
            var routes = new List<CWorkerRoute>();

            for (int i = 0; i < monument.Parts.Length; i++)
            {
                CMonumentPart part = monument.Parts[i];
                if (part.Mode == CMonumentPart.EPartMode.CutoutOnly)
                    continue;
                if (part.Cells == null)
                    continue;

                part.PathToMonument.Bake();
                part.PathFromMonument.Bake();
                CWorkerRoute partRoute = new CWorkerRoute(
                    part.PathToMonument.GetRoute().ToMonument,
                    part.PathFromMonument.GetRoute().ToStorage);

                for (int j = 0; j < part.Cells.Count; j++)
                {
                    if (part.Cells[j] != null)
                    {
                        positions.Add(part.Cells[j].transform.position);
                        routes.Add(partRoute);
                    }
                }
            }

            _cellPositions = positions.ToArray();
            _cellRoutes = routes.ToArray();
            _nextIndex = 0;
            _completedCount = 0;

            Debug.Log($"[CMonumentTaskHandler] Initialized with {_cellPositions.Length} cells.");
        }

        public bool TryAssignTask(out SMonumentTask task)
        {
            if (_cellPositions != null && _nextIndex < _cellPositions.Length)
            {
                task = new SMonumentTask
                {
                    Index = _nextIndex,
                    Target = _cellPositions[_nextIndex],
                    Route = _cellRoutes[_nextIndex]
                };
                _nextIndex++;
                Debug.Log($"[CMonumentTaskHandler] Assigned task {task.Index}.");
                return true;
            }

            task = default;
            return false;
        }

        public void CompleteTask(int index)
        {
            _completedCount++;
            Debug.Log($"[CMonumentTaskHandler] Completed task {index}. Progress: {_completedCount}/{_cellPositions.Length}.");
        }
    }
}
