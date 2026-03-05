// =========================================
// AUTHOR: Juraj Joscak
// DATE:   14.10.2025
// =========================================

using ServerData;

namespace TycoonBuilder.Ui
{
    public class CUiParticleCounts : CUiParticleCountsBase
    {
        public CUiParticleCounts()
        {
            AddCounts
            (
                EValuable.None,
                new[] { 5, 10, 50, 100, 1000, 5000 },
                new[] { 5, 7, 10, 15, 20, 30 }
            );

            AddCounts
            (
                EValuable.SoftCurrency,
                new[] { 100, 500, 1000, 5000, 10000, 50000, 100000 },
                new[] { 3, 5, 7, 10, 20, 35, 50 }
            );

            AddCounts
            (
                EValuable.Fuel,
                new[] { 5, 20, 100, 250, 600, 1000 },
                new[] { 3, 7, 10, 20, 35, 50 }
            );

            AddCounts
            (
                EValuable.HardCurrency,
                new[] { 5, 100, 500, 1000, 2500, 5000, 10000 },
                new[] { 3, 7, 10, 15, 20, 35, 50 }
            );
        }
    }
}