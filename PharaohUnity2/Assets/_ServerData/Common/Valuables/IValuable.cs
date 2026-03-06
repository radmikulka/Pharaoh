// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine;

namespace ServerData
{
    public interface IValuable : IJsonMapAble
    {
        EValuable Id { get; }
        string GetAnalyticsValue() => string.Empty;
    }

    public interface ICountableValuable : IValuable
    {
        int Value { get; }
        ICountableValuable Multiply(int multiplier);
    }
}