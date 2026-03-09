using UnityEngine;

namespace Pharaoh
{
    internal static class CWorkerMovement
    {
        internal static void MoveAlongPath(CWorker worker, Vector3[] waypoints, float dt, float speed)
        {
            float stepLeft = speed * dt;
            while (stepLeft > 0f && worker.WaypointIndex < waypoints.Length)
            {
                Vector3 target = waypoints[worker.WaypointIndex];
                float   dist   = Vector3.Distance(worker.Position, target);

                var dir = target - worker.Position;
                if (dir.sqrMagnitude > 0f)
                    worker.Rotation = Quaternion.LookRotation(dir);

                if (stepLeft >= dist)
                {
                    worker.Position = target;
                    stepLeft       -= dist;
                    worker.WaypointIndex++;
                }
                else
                {
                    worker.Position = Vector3.MoveTowards(worker.Position, target, stepLeft);
                    stepLeft        = 0f;
                }
            }
        }
    }
}
