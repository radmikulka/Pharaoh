using System.Collections.Generic;
using AldaEngine.AldaFramework;
using NaughtyAttributes;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public sealed class CWorkerManager : MonoBehaviour, IInitializable
    {
        private IWorkerConfig     _workerConfig;
        private CMonumentProvider _monumentProvider;
        private CWorkerPath       _path;
        private CWorkerRoute      _route;

        private readonly List<CWorker>         _workers = new();
        private readonly List<CWorkerInstance> _views   = new();

        [Inject]
        private void Construct(IWorkerConfig workerConfig, CMonumentProvider monumentProvider, CWorkerPath path)
        {
            _workerConfig     = workerConfig;
            _monumentProvider = monumentProvider;
            _path             = path;
        }

        public void Initialize()
        {
            var monument = _monumentProvider.Monument;
            if (monument == null || monument.EntryPoint == null)
            {
                Debug.LogWarning("[CWorkerManager] Monument or EntryPoint not assigned.", this);
                return;
            }

            _path.Bake();
            _route = _path.GetRoute();
        }

        private void Update()
        {
            float dt    = Time.deltaTime;
            float speed = _workerConfig.Speed;
            for (int i = 0; i < _workers.Count; i++)
            {
                _workers[i].State = _workers[i].State.Tick(_workers[i], dt, speed);
            }

            SyncViews();
        }

        // ── spawn ────────────────────────────────────────────────────────────

        [Button]
        public void SpawnWorker()
        {
            if (_route == null) return;

            var viewGo = new GameObject($"Worker_{_workers.Count}");
            viewGo.transform.SetParent(transform, false);
            _views.Add(viewGo.AddComponent<CWorkerInstance>());

            _workers.Add(new CWorker
            {
                State         = new CWorkerWalkingToMonumentState(),
                Route         = _route,
                WaypointIndex = 0,
                Position      = transform.position,
                Rotation      = transform.rotation,
            });
        }

        // ── views ────────────────────────────────────────────────────────────

        private void SyncViews()
        {
            for (int i = 0; i < _workers.Count; i++)
            {
                _views[i].Sync(_workers[i].Position, _workers[i].Rotation);
            }
        }
    }
}
