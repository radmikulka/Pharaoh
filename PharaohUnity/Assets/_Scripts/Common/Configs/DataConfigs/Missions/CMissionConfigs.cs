using System;
using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
    public class CMissionConfigs
    {
        private readonly Dictionary<EMissionId, CLazyConfig<CMissionData>> _missions = new();

        protected void AddMission(EMissionId id, Func<CMissionData> config)
        {
            _missions[id] = new CLazyConfig<CMissionData>(config);
        }

        public CMissionData GetMission(EMissionId id)
            => _missions.TryGetValue(id, out var lazy) ? lazy.GetConfig() : null;
    }
}
