using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using NaughtyAttributes;
using ServerData;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
    public sealed class CWorkerManager : MonoBehaviour, IInitializable
    {
        private IWorkerConfig        _workerConfig;
        private CMonumentProvider    _monumentProvider;
        private IEventBus            _eventBus;
        private CMissionController   _missionController;
        private CMonumentTaskHandler _handler;
        private bool                 _initialized;

        private readonly List<CWorker>         _workers = new();
        private readonly List<CWorkerInstance> _views   = new();

        [Inject]
        private void Inject(
            IWorkerConfig        workerConfig,
            CMonumentProvider    monumentProvider,
            IEventBus            eventBus,
            CMissionController   missionController,
            CMonumentTaskHandler handler)
        {
            _workerConfig      = workerConfig;
            _monumentProvider  = monumentProvider;
            _eventBus          = eventBus;
            _missionController = missionController;
            _handler           = handler;
        }

        public void Initialize()
        {
            enabled = false;
            _eventBus.Subscribe<CCoreGameUnlockedSignal>(OnCoreGameUnlocked);
            _eventBus.Subscribe<CMissionStatLevelChangedSignal>(OnMissionStatLevelChanged);
        }

        private void OnCoreGameUnlocked(CCoreGameUnlockedSignal signal)
        {
            var monument = _monumentProvider.Monument;

            _handler.Initialize(monument);
            _initialized = true;

            int count = _missionController.WorkerCountLevel;
            for (int i = 0; i < count; i++)
            {
                SpawnWorker();
            }
            enabled = true;
        }

        private void OnMissionStatLevelChanged(CMissionStatLevelChangedSignal signal)
        {
            if (signal.Stat != EMissionStatId.WorkerCount)
                return;

            int delta = signal.NewLevel - _workers.Count;
            for (int i = 0; i < delta; i++)
            {
                SpawnWorker();
            }
        }

        private void Update()
        {
            float dt    = Time.deltaTime;
            float speed = _workerConfig.Speed;

            for (int i = 0; i < _workers.Count; i++)
            {
                CWorker worker = _workers[i];
                IWorkerState previousState = worker.State;
                worker.State = previousState.Tick(worker, dt, speed);

                if (worker.State is CWorkerIdleState)
                {
                    TryAssignNextTask(worker);
                }

                if (worker.State is CWorkerWalkingToStorageState && previousState is CWorkerDeliveringState)
                {
                    _handler.CompleteTask(worker.CurrentTask.Value.Index);
                    worker.CurrentTask = null;
                }
            }

            SyncViews();
        }

        // ── task assignment ────────────────────────────────────────────────────

        private void TryAssignNextTask(CWorker worker)
        {
            if (_handler.TryAssignTask(out SMonumentTask task))
            {
                worker.CurrentTask = task;
                worker.Route = task.Route;
                worker.State = new CWorkerWalkingToMonumentState();
                worker.WaypointIndex = 0;
            }
        }

        // ── spawn ──────────────────────────────────────────────────────────────

        [Button]
        public void SpawnWorker()
        {
            if (!_initialized) return;

            var viewGo = new GameObject($"Worker_{_workers.Count}");
            viewGo.transform.SetParent(transform, false);
            _views.Add(viewGo.AddComponent<CWorkerInstance>());

            var worker = new CWorker
            {
                State         = new CWorkerIdleState(),
                WaypointIndex = 0,
                Position      = transform.position,
                Rotation      = transform.rotation,
            };

            _workers.Add(worker);
            TryAssignNextTask(worker);
        }

        // ── views ──────────────────────────────────────────────────────────────

        private void SyncViews()
        {
            for (int i = 0; i < _workers.Count; i++)
            {
                _views[i].Sync(_workers[i].Position, _workers[i].Rotation);
            }
        }
    }
}
