// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.10.2025
// =========================================

using System.Collections.Generic;
using ServerData;

namespace Pharaoh.Ui
{
    public class CUiParticleCounts
    {
        private readonly int[] _thresholds = { 5, 100, 500, 1000, 2500, 5000, 10000 };
        private readonly int[] _counts = { 3, 7, 10, 15, 20, 35, 50 };

        public int GetCount(int amount)
        {
            for (int i = _thresholds.Length-1; i >= 0 ; i--)
            {
                if (amount >= _thresholds[i])
                {
                    return _counts[i];
                }
            }
			
            return amount;
        }
    }
}