using System.Collections.Generic;
using AldaEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public sealed class CMonumentTaskHandler
    {
        private IEventBus _eventBus;

        private Vector3[] _cellPositions;
        private CWorkerRoute[] _cellRoutes;
        private int[] _cellPartIndices;
        private CMonumentPart[] _cellParts;
        private int _nextIndex;
        private int _completedCount;
        private int _lastActivatedPartIndex = -1;

        public bool IsComplete => _cellPositions != null && _completedCount >= _cellPositions.Length;

        [Inject]
        private void Inject(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Initialize(CMonumentInstance monument)
        {
            var positions = new List<Vector3>();
            var routes = new List<CWorkerRoute>();
            var partIndices = new List<int>();
            var parts = new List<CMonumentPart>();

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
                        partIndices.Add(i);
                        parts.Add(part);
                    }
                }
            }

            _cellPositions = positions.ToArray();
            _cellRoutes = routes.ToArray();
            _cellPartIndices = partIndices.ToArray();
            _cellParts = parts.ToArray();
            _nextIndex = 0;
            _completedCount = 0;
            _lastActivatedPartIndex = -1;

            if (_cellPositions.Length > 0)
            {
                _lastActivatedPartIndex = _cellPartIndices[0];
                _eventBus.Send(new CMonumentPartActivatedSignal(_lastActivatedPartIndex, _cellParts[0]));
                Debug.Log($"[CMonumentTaskHandler] Activated initial part {_lastActivatedPartIndex}.");
            }

            Debug.Log($"[CMonumentTaskHandler] Initialized with {_cellPositions.Length} cells.");
        }

        public bool TryAssignTask(out SMonumentTask task)
        {
            if (_cellPositions != null && _nextIndex < _cellPositions.Length)
            {
                int partIndex = _cellPartIndices[_nextIndex];
                if (partIndex != _lastActivatedPartIndex)
                {
                    _lastActivatedPartIndex = partIndex;
                    _eventBus.Send(new CMonumentPartActivatedSignal(partIndex, _cellParts[_nextIndex]));
                    Debug.Log($"[CMonumentTaskHandler] Activated part {partIndex}.");
                }

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
