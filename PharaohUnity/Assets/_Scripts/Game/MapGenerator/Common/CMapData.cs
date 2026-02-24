namespace Pharaoh.MapGenerator
{
    public class CMapData
    {
        public int Width { get; }
        public int Height { get; }

        private readonly STile[,] _tiles;

        public CMapData(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new STile[width, height];
        }

        public STile Get(int x, int y) => _tiles[x, y];

        public void Set(int x, int y, STile tile) => _tiles[x, y] = tile;

        public bool IsValid(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
    }
}