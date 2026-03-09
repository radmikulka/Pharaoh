// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.03.2026
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using ServerData.Dto;
using Zenject;

namespace Pharaoh
{
    public sealed class CMissionStats : IInitializable, IDisposable
    {
        public int WorkerCountLevel { get; private set; }
        public int WorkerSpeedLevel { get; private set; }
        public int ProfitLevel      { get; private set; }

        private IEventBus                  _eventBus;
        private IMissionStatLimitsProvider _limits;
        private Guid _upgradedSub;

        [Inject]
        private void Inject(IEventBus eventBus, IMissionStatLimitsProvider limits)
        {
            _eventBus = eventBus;
            _limits   = limits;
        }

        public void Initialize()
        {
            _upgradedSub = _eventBus.Subscribe<CMissionStatUpgradedSignal>(OnStatUpgraded);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe(_upgradedSub);
        }

        public int GetLevel(EMissionStatId stat) => stat switch
        {
            EMissionStatId.WorkerCount => WorkerCountLevel,
            EMissionStatId.WorkerSpeed => WorkerSpeedLevel,
            EMissionStatId.Profit      => ProfitLevel,
            _                          => throw new ArgumentOutOfRangeException(nameof(stat))
        };

        public CMissionStatsDto ToDto() => new()
        {
            WorkerCountLevel = WorkerCountLevel,
            WorkerSpeedLevel = WorkerSpeedLevel,
            ProfitLevel      = ProfitLevel,
        };

        private void OnStatUpgraded(CMissionStatUpgradedSignal signal)
        {
            int maxLevel = _limits.GetMaxLevel(signal.Stat);
            int current  = GetLevel(signal.Stat);
            if (current >= maxLevel)
                return;

            switch (signal.Stat)
            {
                case EMissionStatId.WorkerCount: WorkerCountLevel++; break;
                case EMissionStatId.WorkerSpeed: WorkerSpeedLevel++; break;
                case EMissionStatId.Profit:      ProfitLevel++;      break;
            }
            _eventBus.Send(new CMissionStatLevelChangedSignal(signal.Stat, GetLevel(signal.Stat)));
        }
    }
}
