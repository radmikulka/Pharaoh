using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace GiantGrey.TileWorldCreator.Samples
{
    /// <summary>
    /// Simply script which generates a new path and gets the resulting cells from the path blueprint layer.
    /// Those resulting cell positions are then used as the follow path.
    /// </summary>
    public class FollowPath : MonoBehaviour
    {
        public TileWorldCreatorManager tileWorldCreator;
        public float movementSpeed = 5f;
        public List<Vector2> path;

        int waypoint = 0;
        bool pathReady = false;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);

            Debug.Log("GENERATING PATH");
            // Generate new map with new path
            tileWorldCreator.ExecuteBlueprintLayers();
        }

        void OnEnable()
        {

            tileWorldCreator.OnBlueprintLayersReady += LayersReady;
        }

        void OnDisable()
        {
            tileWorldCreator.OnBlueprintLayersReady -= LayersReady;
        }


        // Generation done, get the resulting path and build layers
        void LayersReady()
        {
            GetPath();

            tileWorldCreator.ExecuteBuildLayers(ExecutionMode.FromScratch);
        }


        public void GetPath()
        {
            waypoint = 0;
            path.Clear();

            // Get the blueprint layer named path
            var _pathLayer = tileWorldCreator.GetBlueprintLayer("Path");


             HashSet<Vector2> _positions = new HashSet<Vector2>();

            // A. Either get the resulting cell positions from the blueprint layer
            _positions = _pathLayer.GetAllCellPositions(_positions);

            // OR
            // B. Get the pathfinding modifier on the blueprint layer
            // var _pathfinding = _pathLayer.GetModifier<Pathfinding>(0);
            // And use FindPath to get the resulting cell positions
            // var _p = _pathfinding.FindPath(out _positions);

            // Assign to path list
            path = _positions.ToList();

            // Does the path has any positions?
            if (path.Count > 0) // or check if path has any positions (path.Count > 0)
            {
                Debug.Log("PATH READY");
                pathReady = true;
                // place this object to start position
                this.transform.position = new Vector3(path[0].x, 0, path[0].y);
            }
            else
            {
                Debug.Log("No path found, trying again");

                // A. Generate a new map with new path and wait for OnBlueprintLayersReady event
                tileWorldCreator.ExecuteBlueprintLayers();

            }

        }

        IEnumerator FindPathIE()
        {
            yield return new WaitForSeconds(1f);
            GetPath();
        }

        public void Update()
        {
            if (!pathReady)
                return;

            // Move along path
            this.transform.position = Vector3.MoveTowards(transform.position, new Vector3(path[waypoint].x, 0, path[waypoint].y), Time.deltaTime * movementSpeed);

            if (Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), path[waypoint]) < 0.1f)
            {
                if (waypoint + 1 < path.Count)
                {
                    // Jump to next path position if distance is less than 0.1
                    waypoint++;
                }
                else
                {
                    pathReady = false;

                    // end reached
                    // Generate new map with new path
                    tileWorldCreator.ExecuteBlueprintLayers();
                }
            }
        }

        // Draw debug path
        void OnDrawGizmos()
        {
            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    if (i + 1 >= path.Count)
                        break;

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(new Vector3(path[i].x, 0, path[i].y), new Vector3(path[i + 1].x, 0, path[i + 1].y));
                }
            }
        }
    }
}