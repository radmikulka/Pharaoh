// // =========================================
// // AUTHOR: Radek Mikulka
// // DATE:   06.09.2023
// // =========================================

using System.Numerics;
using JetBrains.Annotations;

namespace Pharaoh
{
    public struct SValueChangeArgs
    {
        [CanBeNull] public readonly CValueModifyParams ModifyParams;
        public readonly int PreviousValue;
        public readonly int NewValue;
        public int Difference => NewValue - PreviousValue;

        public SValueChangeArgs(CValueModifyParams modifyParams, int previousValue, int newValue)
        {
            ModifyParams = modifyParams;
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}