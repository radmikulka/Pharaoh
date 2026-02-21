// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using System.Collections.Generic;
using ServerData;
using UnityEngine;

namespace Pharaoh
{
    public class CUiCurrencyParticles
    {
        private readonly Dictionary<EValuable, CUiCurrencyParticlePoint> _currencies = new();
        
        public void RegisterCurrency(CUiCurrencyParticlePoint currency)
        {
            _currencies.Add(currency.CurrencyId, currency);
        }
        
        public CUiCurrencyParticlePoint GetCurrency(EValuable id)
        {
            return _currencies[id];
        }
    }
}