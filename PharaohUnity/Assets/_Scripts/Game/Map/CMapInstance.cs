using System.Collections.Generic;
using Pharaoh.MapGenerator;
using UnityEngine;

namespace Pharaoh.Map
{
    public class CMapInstance : MonoBehaviour
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private CMapCell[,] _cells;

        private static readonly (int dx, int dy)[] CardinalOffsets = { (0, 1), (0, -1), (1, 0), (-1, 0) };

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new CMapCell[width, height];
        }

        public void SetCell(int x, int y, CMapCell cell) => _cells[x, y] = cell;

        public CMapCell GetCell(int x, int y) => _cells[x, y];

        public bool IsValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public CMapCell GetCellByWorldPos(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x);
            int y = Mathf.RoundToInt(worldPos.z);
            if (!IsValid(x, y))
                return null;
            return _cells[x, y];
        }

        public IEnumerable<CMapCell> GetNeighbors(int x, int y)
        {
            foreach (var (dx, dy) in CardinalOffsets)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (IsValid(nx, ny))
                    yield return _cells[nx, ny];
            }
        }

        public void ComputeCellTags()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CMapCell cell = _cells[x, y];
                    if (!cell.TileType.IsBuildable())
                        continue;

                    ECellTag tags = ECellTag.None;

                    foreach (CMapCell neighbor in GetNeighbors(x, y))
                    {
                        if (neighbor.TileType == ETileType.Water)
                            tags |= ECellTag.NearWater | ECellTag.Coastal;

                        if (neighbor.TileType == ETileType.Coast)
                            tags |= ECellTag.NearSand;

                        if (neighbor.ObstacleObject != null)
                            tags |= ECellTag.NearRock;
                    }

                    cell.Tags = tags;
                }
            }
        }
    }
}
