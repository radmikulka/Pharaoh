// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using System;
using System.Collections.Generic;
using ServerData;
using ServerData.Dto;

namespace Pharaoh
{
    public sealed class CMissionData
    {
        public int   WorkersCount  { get; internal set; }
        public int   WorkerSpeedLevel  { get; internal set; }
        public int   ProfitLevel       { get; internal set; }
        public int   SoftCurrency      { get; internal set; }
        public float MonumentProgress  { get; internal set; }

        internal readonly HashSet<float> ClaimedMilestones = new();

        public int GetLevel(EMissionStatId stat) => stat switch
        {
            EMissionStatId.WorkerCount => WorkersCount,
            EMissionStatId.WorkerSpeed => WorkerSpeedLevel,
            EMissionStatId.Profit      => ProfitLevel,
            _                          => throw new ArgumentOutOfRangeException(nameof(stat))
        };

        public CMissionDataDto ToDto() => new()
        {
            WorkerCountLevel = WorkersCount,
            WorkerSpeedLevel = WorkerSpeedLevel,
            ProfitLevel      = ProfitLevel,
            SoftCurrency     = SoftCurrency,
        };
    }
}
