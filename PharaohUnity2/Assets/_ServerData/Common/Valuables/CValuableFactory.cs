// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Numerics;

namespace ServerData
{
    public static class CValuableFactory
    {
        public static CConsumableValuable Consumable(EValuable currencyId, int amount) => new(currencyId, amount);
        public static CConsumableValuable HardCurrency(int count) => new(EValuable.HardCurrency, count);
        public static CNullValuable Null => CNullValuable.Instance;
    }
}