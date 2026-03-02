// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;

namespace Pharaoh
{
    public class CUser : IDestroyable
    {
        public readonly COwnedValuables OwnedValuables;
        public readonly COwnedResources OwnedResources;
        public readonly CActiveMission ActiveMission;
        public readonly CAnimatedCurrencies AnimatedCurrencies;
        public readonly COwnedResearches OwnedResearches;

        public bool IsValid { get; private set; }

        private readonly IEventBus _eventBus;
        private readonly List<CBaseUserComponent> _allComponents = new();

        public CUser(
            CAnimatedCurrencies animatedCurrencies,
            CInitialUserDataProvider dataProvider,
            COwnedValuables ownedValuables,
            COwnedResources ownedResources,
            CActiveMission activeMission,
            COwnedResearches ownedResearches,
            IEventBus eventBus
            )
        {
            AnimatedCurrencies = animatedCurrencies;
            OwnedValuables = ownedValuables;
            OwnedResources = ownedResources;
            ActiveMission = activeMission;
            OwnedResearches = ownedResearches;
            _eventBus = eventBus;

            _allComponents.Add(OwnedValuables);
            _allComponents.Add(OwnedResources);
            _allComponents.Add(AnimatedCurrencies);
            _allComponents.Add(ActiveMission);
            _allComponents.Add(OwnedResearches);

            InitComponents();
            InitialSync(dataProvider.Dto);
        }

        private void InitComponents()
        {
            foreach (CBaseUserComponent component in _allComponents)
            {
                component.Initialize(this);
            }
        }
        private void InitialSync(CUserDto dto)
        {
            IsValid = dto != null;

            if(!IsValid)
                return;

            OwnedValuables.InitialSync(dto.OwnedValuables);
            ActiveMission.InitialSync(dto.ActiveMission);
            OwnedResearches.InitialSync();
        }

        public void OnContextDestroy(bool appExits)
        {
            Dispose();
        }

        private void Dispose()
        {
            foreach (CBaseUserComponent component in _allComponents)
            {
                component.Dispose();
            }
        }
    }
}