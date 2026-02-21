// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

namespace ServerData
{
    public struct SResource
    {
        public readonly EResource Id;
        public readonly int Amount;

        public SResource(EResource id, int amount)
        {
            Id = id;
            Amount = amount;
        }
        
        public SResource GetNegative()
        {
            return new SResource(Id, -Amount);
        }
        
        public static SResource operator *(SResource a, float multiplier)
        {
            return new SResource(a.Id, (int)(a.Amount * multiplier));
        }
    }
}