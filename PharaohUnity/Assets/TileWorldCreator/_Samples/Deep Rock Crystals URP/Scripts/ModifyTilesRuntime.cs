using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GiantGrey.TileWorldCreator.Samples
{

    /// <summary>
    /// This is a very simple sample which demonstrates how you can modify cells on blueprint layers at runtime
    /// </summary>
    public class ModifyTilesRuntime : MonoBehaviour
    {
        public TileWorldCreatorManager tileWorldCreatorManager;
        public string crystalsLayerName = "Crystals Paint";
        public string cliffTilesLayerName = "Cliffs Paint";
        public string brokenCliffTilesLayerName = "Broken Cliffs Paint";
        public string buildCliffTilesLayerName = "Cliffs Build";

        public GameObject slashEffect;
        public GameObject crystalCollectFX;
        public GameObject breakFX;
        public float attackRange = 2f;



        void Update()
        {

            if (EventSystem.current.IsPointerOverGameObject())
                return;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current.leftButton.wasReleasedThisFrame)
#else
            if (Input.GetMouseButtonUp(0))
#endif
            {
                // Spawn slash FX
                var _slashEffect = Instantiate(slashEffect, transform.position, Quaternion.Euler(90, Random.Range(0, 360), 0));
                Destroy(_slashEffect, 0.5f);

                // Collect all tiles in attack range
                var _cells = tileWorldCreatorManager.GetCellPositionsInRadius(cliffTilesLayerName, transform.position, attackRange);

                
                // Collect all Crystals in attack range
                var _crystals = tileWorldCreatorManager.GetCellPositionsInRadius(crystalsLayerName, transform.position, attackRange);
                foreach (var _crystal in _crystals)
                {
                    // For all crystals in attack range, remove them and spawn collect fx
                    tileWorldCreatorManager.RemoveCellsFromLayer(crystalsLayerName, new HashSet<Vector2>() { _crystal });
                    var _tmpCrystal = Instantiate(crystalCollectFX, new Vector3(_crystal.x * tileWorldCreatorManager.configuration.cellSize, 1f, _crystal.y * tileWorldCreatorManager.configuration.cellSize), Quaternion.identity);
                    Destroy(_tmpCrystal, 2.5f);
                }

                // Destroy normal cliff tiles in range
                tileWorldCreatorManager.RemoveCellsFromLayer(cliffTilesLayerName, _cells);


                // Get all existing broken tiles in attack range
                var _brokenCells = tileWorldCreatorManager.GetCellPositionsInRadius(brokenCliffTilesLayerName, transform.position, attackRange);

                // Check which ones are new
                HashSet<Vector2> newBrokenTiles = new HashSet<Vector2>();
                foreach (var _cell in _cells)
                {
                    // Check if a "normal" tile is already added to the broken tiles
                    // if not add it and spawn break fx
                    if (!_brokenCells.Contains(_cell))
                    {
                        newBrokenTiles.Add(_cell);

                        var _breakFX1 = Instantiate(breakFX, new Vector3(_cell.x * tileWorldCreatorManager.configuration.cellSize, 1.5f, _cell.y * tileWorldCreatorManager.configuration.cellSize), Quaternion.identity);
                        Destroy(_breakFX1, 1f);
                    }
                }

                // For all new broken tiles add them to the broken tiles layer
                if (newBrokenTiles.Count > 0)
                {
                    tileWorldCreatorManager.AddCellsToLayer(brokenCliffTilesLayerName, newBrokenTiles);
                }


                // Get DamagedBlockFXs component on broken cliff tiles to modify the health value
                var _colliders = Physics.OverlapSphere(this.transform.position, attackRange);

                HashSet<Vector2> _destroyedCliffs = new HashSet<Vector2>();

                for (int i = 0; i < _colliders.Length; i++)
                {
                    var _comp = _colliders[i].GetComponent<DamageHitFX>();
                    if (_comp != null)
                    {

                        // Modify health value. If less than 0, get the tiles world position and add it to the positions list so we can remove them from the broken cliffs tiles layer.
                        if (_comp.Hit() <= 0)
                        {
                            var _p = tileWorldCreatorManager.GetBuildLayerTileDataFromPosition(buildCliffTilesLayerName, new Vector2(_comp.transform.position.x, _comp.transform.position.z));
                            _destroyedCliffs.Add(_p.worldMapPosition);
                        }

                        // Spawn particle break FX
                        var _breakFX2 = Instantiate(breakFX, new Vector3(_comp.transform.position.x, _comp.transform.position.y + 0.4f, _comp.transform.position.z), Quaternion.identity);
                        Destroy(_breakFX2, 1f);
                    }
                }



                tileWorldCreatorManager.RemoveCellsFromLayer(brokenCliffTilesLayerName, _destroyedCliffs);


                if (newBrokenTiles.Count > 0 || _destroyedCliffs.Count > 0)
                {
                    tileWorldCreatorManager.ExecuteBuildLayers(ExecutionMode.Normal);
                }


            }
        }
    }
}