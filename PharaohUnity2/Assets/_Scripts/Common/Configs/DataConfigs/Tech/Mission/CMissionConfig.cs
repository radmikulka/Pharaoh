// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

namespace Pharaoh
{
    public class CMissionConfig
    {
        public int MaxWorkerCount { get; }

        public CMissionConfig(int maxWorkerCount)
        {
            MaxWorkerCount = maxWorkerCount;
        }
    }
}
