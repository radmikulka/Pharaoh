// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Numerics;
using Newtonsoft.Json;

namespace ServerData
{
    public class CConsumableValuable : ICountableValuable
    {
        public EValuable Id { get; }
        public int Value { get; set; }

        public CConsumableValuable()
        {
        }

        [JsonConstructor]
        public CConsumableValuable(EValuable id, int value)
        {
            Id = id;
            Value = value;
        }

        public CConsumableValuable Reverse()
        {
            return new CConsumableValuable(Id, -Value);
        }

        public CConsumableValuable Double()
        {
            return new CConsumableValuable(Id, Value * 2);
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Value)}: {Value}";
        }
    }
}