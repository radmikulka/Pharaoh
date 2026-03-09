// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using System;
using System.Collections.Generic;

namespace Pharaoh
{
    public class CMissionConfig
    {
        public int MaxWorkerCount { get; }
        public IReadOnlyList<SMilestoneConfig> Milestones { get; }

        public CMissionConfig(int maxWorkerCount, SMilestoneConfig[] milestones = null)
        {
            MaxWorkerCount = maxWorkerCount;
            Milestones     = milestones ?? Array.Empty<SMilestoneConfig>();
        }
    }
}
