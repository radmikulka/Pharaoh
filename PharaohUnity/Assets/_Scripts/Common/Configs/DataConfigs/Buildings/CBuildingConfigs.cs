using System;
using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
    public class CBuildingConfigs
    {
        private readonly Dictionary<EBuildingId, CLazyConfig<CBuildingConfig>> _buildings = new();
        
        protected void AddBuilding(EBuildingId id, Func<CBuildingConfig> config)
        {
            _buildings[id] = new CLazyConfig<CBuildingConfig>(config);
        }

        public CBuildingConfig GetBuilding(EBuildingId id)
            => _buildings.TryGetValue(id, out var lazy) ? lazy.GetConfig() : null;
    }
}