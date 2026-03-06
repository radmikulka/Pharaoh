// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Numerics;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder;
using UnityEngine;

namespace TycoonBuilder
{
    public class CAnimatedCurrencies : CBaseUserComponent, ITickable
    {
        private readonly Dictionary<EValuable, CAnimatedCurrency> _currencies = new();

        private readonly IServerTime _serverTime;
        private readonly IEventBus _eventBus;
        public CAnimatedCurrencies(IServerTime serverTime, IEventBus eventBus)
        {
            _serverTime = serverTime;
            _eventBus = eventBus;
        }
        
        public override void Initialize(CUser user)
        {
            base.Initialize(user);
            
            AddCurrency(EValuable.HardCurrency);
        }

        public void Tick()
        {
            foreach (CAnimatedCurrency currency in _currencies.Values)
            {
                currency.Tick(_serverTime.GetTimestampInMs());
            }
        }

        private void AddCurrency(EValuable valuableId)
        {
            CConsumableOwnedValuable valuable = User.OwnedValuables.GetValuable<CConsumableOwnedValuable>(valuableId);
            CAnimatedValuable animatedValuable = new(valuable);
            _currencies.Add(valuableId, new CAnimatedCurrency(animatedValuable));
        }

        public void AddCurrency(CConsumableValuable valuable)
        {
            GetCurrency(valuable.Id).AddValue(valuable.Value);
        }
        
        public void StartAnimating(EValuable valuable, CLockObject lockObject)
        {
            GetCurrency(valuable).AddBidingLock(lockObject);
        }
        
        public void StopAnimating(EValuable valuable, CLockObject lockObject)
        {
            GetCurrency(valuable).RemoveBidingLock(lockObject);
        }
        
        public CAnimatedCurrency GetCurrency(EValuable valuable)
        {
            return _currencies[valuable];
        }
        
        public override void Dispose()
        {
            base.Dispose();

            foreach (var currency in _currencies)
            {
                currency.Value.Dispose();
            }
        }
    }
}