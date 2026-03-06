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
    public class CUser : IDestroyable, IInitializable
    {
        public readonly CAnimatedCurrencies AnimatedCurrencies;
        public readonly COwnedValuables OwnedValuables;
        public readonly CUserProgress Progress;
        public readonly CAccount Account;

        public bool IsValid { get; private set; }

        private readonly IEventBus _eventBus;
        private readonly CServerResponseDataSync _dataSync = new();
        private readonly List<CBaseUserComponent> _allComponents = new();

        public CUser(
            CAnimatedCurrencies animatedCurrencies,
            CInitialUserDtoProvider dtoProvider,
            COwnedValuables ownedValuables,
            CUserProgress progress,
            IEventBus eventBus,
            CAccount account
            )
        {
            AnimatedCurrencies = animatedCurrencies;
            OwnedValuables = ownedValuables;
            _eventBus = eventBus;
            Progress = progress;
            Account = account;

            _allComponents.Add(Progress);
            _allComponents.Add(OwnedValuables);
            _allComponents.Add(Account);
            _allComponents.Add(AnimatedCurrencies);

            InitComponents();
            InitialSync(dtoProvider.Dto);
        }

        public void Initialize()
        {
            _eventBus.Subscribe<CServerHitsProcessedSignal>(OnHitsProcessed);
        }

        private void OnHitsProcessed(CServerHitsProcessedSignal signal)
        {
            _dataSync.SyncHits(signal.Hits, this);
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

            Progress.InitialSync(dto.Progress);
            OwnedValuables.InitialSync(dto.OwnedValuables);
            Account.InitialSync(dto.Account);
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
