// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using System;
using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
    public class CBaseMissionConfigs
    {
        private readonly Dictionary<EMissionId, CLazyConfig<CMissionConfig>> _missions = new();

        protected void AddMission(EMissionId id, Func<CMissionConfig> config)
        {
            _missions[id] = new CLazyConfig<CMissionConfig>(config);
        }

        public CMissionConfig GetMission(EMissionId id)
            => _missions.TryGetValue(id, out var lazy) ? lazy.GetConfig() : null;
    }
}
