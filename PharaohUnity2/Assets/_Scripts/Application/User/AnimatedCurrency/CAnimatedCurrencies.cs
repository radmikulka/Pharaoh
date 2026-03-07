// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using System.Numerics;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using Pharaoh;
using UnityEngine;

namespace Pharaoh
{
    public class CAnimatedCurrencies : CBaseUserComponent
    {
        private readonly Dictionary<EValuable, CAnimatedCurrency> _currencies = new();
        
        public override void Initialize(CUser user)
        {
            base.Initialize(user);
            
            AddCurrency(EValuable.HardCurrency);
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
            GetCurrency(valuable).AddAnimationLock(lockObject);
        }
        
        public void StopAnimating(EValuable valuable, CLockObject lockObject)
        {
            GetCurrency(valuable).RemoveAnimationLock(lockObject);
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