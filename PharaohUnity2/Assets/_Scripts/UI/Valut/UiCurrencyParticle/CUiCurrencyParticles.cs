// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
    public class CUiCurrencyParticles
    {
        private readonly Dictionary<(EValuable, EResource), CUiCurrencyParticlePoint> _currencies = new();
        private readonly Dictionary<ELiveEvent, RectTransform> _liveEventPoints = new();
        
        public void RegisterCurrency(CUiCurrencyParticlePoint currency)
        {
            _currencies.Add((currency.CurrencyId, currency.CurrencyId == EValuable.Resource ? currency.ResourceId : EResource.None), currency);
        }

        public void RegisterEvent(ELiveEvent liveEvent, RectTransform currencyPoint)
        {
            _liveEventPoints.Remove(liveEvent);
            _liveEventPoints.Add(liveEvent, currencyPoint);
        }

        public RectTransform GetLiveEvent(ELiveEvent liveEvent)
        {
            return _liveEventPoints.GetValueOrDefault(liveEvent);
        }
        
        public CUiCurrencyParticlePoint GetCurrency(EValuable id)
        {
            return _currencies[(id, EResource.None)];
        }
        
        public CUiCurrencyParticlePoint GetCurrency(EValuable id, EResource resourceId)
        {
            if(_currencies.TryGetValue((id, resourceId), out CUiCurrencyParticlePoint currency))
                return currency;
            
            return _currencies[(id, EResource.None)];
        }
    }
}