// =========================================
// AUTHOR: Marek Karaba
// DATE:   19.02.2026
// =========================================

using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiEventPointParticlePoint : ValidatedMonoBehaviour, IAldaFrameworkComponent
	{
		private CUiCurrencyParticles _currencyParticles;
        
		[Inject]
		private void Inject(CUiCurrencyParticles currencyParticles)
		{
			_currencyParticles = currencyParticles;
		}

		public void Register(ELiveEvent liveEvent)
		{
			_currencyParticles.RegisterEvent(liveEvent, transform as RectTransform);
		}
	}
}