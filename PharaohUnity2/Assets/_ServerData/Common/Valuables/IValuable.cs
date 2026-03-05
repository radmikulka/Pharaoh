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
        EValuablePrice GetPriceType() => EValuablePrice.None;
        string GetAnalyticsValue() => string.Empty;
    }

    public interface IOfferAnalyticsValueProvider
    {
        string GetOfferRewardAnalyticsValue();
    }

    public interface ICountableValuable : IValuable, IOfferAnalyticsValueProvider
    {
        int Value { get; }
        ICountableValuable Multiply(int multiplier);
    }
}