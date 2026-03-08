using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pharaoh.CoreGame
{
    public sealed class CWorkerManager : IInitializable, ITickable, IDisposable
    {
        private const float WorkerSpeed            = 5f;
        private const float DeliveryPauseDuration  = 0.5f;
        private const int   WorkersPerMonument      = 40;

        private readonly CWorkerRouteComputer _routeComputer = new();
        private readonly List<CWorker>        _workers       = new();
        private readonly List<GameObject>     _viewObjects   = new();

        public void Initialize()
        {
            var storageGo = GameObject.FindWithTag("Storage");
            if (storageGo == null)
            {
                Debug.LogWarning("[CWorkerManager] No GameObject tagged 'Storage' found in scene.");
                return;
            }

            var monuments = Object.FindObjectsOfType<CMonumentInstance>();
            if (monuments.Length == 0)
            {
                Debug.LogWarning("[CWorkerManager] No CMonumentInstance found in scene.");
                return;
            }

            var storagePos = storageGo.transform.position;

            foreach (var monument in monuments)
            {
                if (monument.EntryPoint == null)
                {
                    Debug.LogWarning($"[CWorkerManager] CMonumentInstance '{monument.name}' has no EntryPoint assigned.", monument);
                    continue;
                }

                var route = _routeComputer.GetOrCompute(storagePos, monument.EntryPoint.position);

                for (int i = 0; i < WorkersPerMonument; i++)
                {
                    var viewGo = new GameObject($"Worker_{_workers.Count}");
                    viewGo.AddComponent<CWorkerView>();
                    _viewObjects.Add(viewGo);

                    var worker = new CWorker
                    {
                        State        = EWorkerState.WalkingToMonument,
                        Route        = route,
                        WaypointIndex = 0,
                        Position     = storagePos,
                        PauseTimer   = 0f,
                        ViewIndex    = _viewObjects.Count - 1,
                    };

                    // Stagger start positions along the path to avoid all workers clumping
                    float staggerT = (float)i / WorkersPerMonument;
                    StaggerWorkerAlongPath(worker, route.ToMonument, staggerT);

                    _workers.Add(worker);
                }
            }
        }

        public void Tick()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _workers.Count; i++)
                UpdateWorker(_workers[i], dt);

            SyncViews();
        }

        public void Dispose()
        {
            foreach (var go in _viewObjects)
            {
                if (go != null)
                    Object.Destroy(go);
            }
            _workers.Clear();
            _viewObjects.Clear();
            _routeComputer.ClearCache();
        }

        // ── per-worker update ────────────────────────────────────────────────

        private void UpdateWorker(CWorker w, float dt)
        {
            switch (w.State)
            {
                case EWorkerState.IdleAtStorage:
                    w.State        = EWorkerState.WalkingToMonument;
                    w.WaypointIndex = 0;
                    break;

                case EWorkerState.WalkingToMonument:
                    MoveWorker(w, w.Route.ToMonument, dt);
                    if (w.WaypointIndex >= w.Route.ToMonument.Length)
                    {
                        w.State      = EWorkerState.Delivering;
                        w.PauseTimer = 0f;
                    }
                    break;

                case EWorkerState.Delivering:
                    w.PauseTimer += dt;
                    if (w.PauseTimer >= DeliveryPauseDuration)
                    {
                        w.State        = EWorkerState.WalkingToStorage;
                        w.WaypointIndex = 0;
                    }
                    break;

                case EWorkerState.WalkingToStorage:
                    MoveWorker(w, w.Route.ToStorage, dt);
                    if (w.WaypointIndex >= w.Route.ToStorage.Length)
                    {
                        w.State        = EWorkerState.WalkingToMonument;
                        w.WaypointIndex = 0;
                    }
                    break;
            }
        }

        private static void MoveWorker(CWorker w, Vector3[] waypoints, float dt)
        {
            float stepLeft = WorkerSpeed * dt;
            while (stepLeft > 0f && w.WaypointIndex < waypoints.Length)
            {
                float dist = Vector3.Distance(w.Position, waypoints[w.WaypointIndex]);
                if (stepLeft >= dist)
                {
                    w.Position = waypoints[w.WaypointIndex++];
                    stepLeft  -= dist;
                }
                else
                {
                    w.Position = Vector3.MoveTowards(w.Position, waypoints[w.WaypointIndex], stepLeft);
                    stepLeft   = 0f;
                }
            }
        }

        private void SyncViews()
        {
            for (int i = 0; i < _workers.Count; i++)
            {
                var w  = _workers[i];
                var go = _viewObjects[w.ViewIndex];
                if (go != null)
                    go.transform.position = w.Position;
            }
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static void StaggerWorkerAlongPath(CWorker w, Vector3[] waypoints, float t)
        {
            if (waypoints.Length == 0) return;

            float totalLength = 0f;
            for (int i = 0; i < waypoints.Length - 1; i++)
                totalLength += Vector3.Distance(waypoints[i], waypoints[i + 1]);

            float target = t * totalLength;
            float walked = 0f;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                float seg = Vector3.Distance(waypoints[i], waypoints[i + 1]);
                if (walked + seg >= target)
                {
                    float along = target - walked;
                    w.Position     = Vector3.Lerp(waypoints[i], waypoints[i + 1], along / seg);
                    w.WaypointIndex = i + 1;
                    return;
                }
                walked += seg;
            }

            w.Position     = waypoints[waypoints.Length - 1];
            w.WaypointIndex = waypoints.Length;
        }
    }
}
