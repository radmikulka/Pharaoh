// =========================================
// AUTHOR: Radek Mikulka
// DATE:   09.07.2025
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
    public class CUiCurrencyParticlePoint : ValidatedMonoBehaviour, IInitializable
    {
        [SerializeField, Self] private RectTransform _rectTransform;
        [SerializeField] private EValuable _currencyId;
        [SerializeField] private EResource _resourceId;
        [SerializeField] private EUiParticleId _particleId;

        private CUiCurrencyParticles _currencyParticles;

        public RectTransform RectTransform => _rectTransform;
        public EUiParticleId ParticleId => _particleId;
        public EValuable CurrencyId => _currencyId;
        public EResource ResourceId => _resourceId;

        [Inject]
        private void Inject(CUiCurrencyParticles currencyParticles)
        {
            _currencyParticles = currencyParticles;
        }

        public void Initialize()
        {
            _currencyParticles.RegisterCurrency(this);
        }
    }
}