// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Numerics;

namespace ServerData
{
    public static class CValuableFactory
    {
        public static CConsumableValuable HardCurrency(int count) => new(EValuable.HardCurrency, count);
        public static CConsumableValuable Consumable(EValuable id, int count) => new(id, count);
        public static CRealMoneyValuable RealMoney(EInAppPrice price) { return new(price); }
        public static CFreeValuable Free => CFreeValuable.Instance;
        public static CNullValuable Null => CNullValuable.Instance;
    }
}