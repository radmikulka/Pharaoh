using System.Collections.Generic;
using UnityEngine;

namespace Pharaoh.MapGenerator
{
    /// <summary>
    /// Carves natural-looking river paths through land tiles using noise-based lateral drift.
    /// Rivers flow from one map edge to the opposite edge.
    /// </summary>
    public class CRiverGenerationStep : MonoBehaviour, IMapGenerationStep
    {
        [Header("Rivers")]
        [SerializeField] [Range(0, 3)] private int _riverCount = 1;
        [SerializeField] [Range(1, 2)] private int _maxWidth = 1;

        [Header("Path Shape")]
        [SerializeField] [Range(0f, 1f)] private float _windiness = 0.5f;
        [SerializeField] private float _noiseFrequency = 0.05f;

        public string StepName => "Rivers";

        public void Execute(CMapData mapData, int seed)
        {
            if (_riverCount <= 0) return;

            var rng = new System.Random(seed);

            for (int i = 0; i < _riverCount; i++)
            {
                int riverSeed = rng.Next();
                GenerateRiver(mapData, rng, riverSeed);
            }
        }

        private void GenerateRiver(CMapData mapData, System.Random rng, int riverSeed)
        {
            // Determine flow direction: 0 = north-south, 1 = east-west
            bool horizontal = rng.Next(2) == 0;

            int length = horizontal ? mapData.Width : mapData.Height;
            int breadth = horizontal ? mapData.Height : mapData.Width;

            // Pick start position along the entry edge
            int startCross = FindLandOnEdge(mapData, rng, horizontal, isEnd: false);
            if (startCross < 0) return;

            // Set up noise for lateral drift
            var noise = new FastNoiseLite(riverSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(_noiseFrequency);

            float crossPos = startCross;
            int placed = 0;

            for (int step = 0; step < length; step++)
            {
                // Sample noise for lateral drift
                float raw = noise.GetNoise(step, 0f);
                float drift = raw * _windiness * 2f;
                crossPos += drift;
                crossPos = Mathf.Clamp(crossPos, 0, breadth - 1);

                int crossInt = Mathf.RoundToInt(crossPos);

                // Determine tile coordinates based on flow direction
                int x = horizontal ? step : crossInt;
                int y = horizontal ? crossInt : step;

                if (!mapData.IsValid(x, y)) continue;

                // Carve main tile
                CarveTile(mapData, x, y);
                placed++;

                // Widen river
                if (_maxWidth > 1)
                {
                    int wx = horizontal ? x : x + 1;
                    int wy = horizontal ? y + 1 : y;
                    if (mapData.IsValid(wx, wy))
                        CarveTile(mapData, wx, wy);
                }
            }

            Debug.Log($"[{StepName}] River carved: {placed} tiles (direction: {(horizontal ? "E-W" : "N-S")}).");
        }

        private int FindLandOnEdge(CMapData mapData, System.Random rng, bool horizontal, bool isEnd)
        {
            int breadth = horizontal ? mapData.Height : mapData.Width;
            int edgePos = isEnd
                ? (horizontal ? mapData.Width - 1 : mapData.Height - 1)
                : 0;

            // Collect all land tiles on this edge
            var candidates = new List<int>();
            for (int i = 0; i < breadth; i++)
            {
                int x = horizontal ? edgePos : i;
                int y = horizontal ? i : edgePos;
                if (mapData.IsValid(x, y) && mapData.Get(x, y).Type == ETileType.Land)
                    candidates.Add(i);
            }

            if (candidates.Count == 0) return -1;
            return candidates[rng.Next(candidates.Count)];
        }

        private static void CarveTile(CMapData mapData, int x, int y)
        {
            STile tile = mapData.Get(x, y);
            if (tile.Type == ETileType.Land)
            {
                tile.Type = ETileType.Water;
                mapData.Set(x, y, tile);
            }
        }
    }
}
