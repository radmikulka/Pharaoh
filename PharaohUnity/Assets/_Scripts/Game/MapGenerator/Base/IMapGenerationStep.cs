namespace Pharaoh.MapGenerator
{
    public interface IMapGenerationStep
    {
        string StepName { get; }

        void Execute(CMapData mapData, int seed);
    }
}