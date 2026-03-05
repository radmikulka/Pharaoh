// =========================================
// AUTHOR: Marek Karaba
// DATE:   14.01.2026
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiCurrencyTopBarItem : CUiTopBarItem
	{
		[SerializeField] private CUiComponentText _text;
		private ITopBarItemWithMaxCapacity _maximumCapacity;
		
		private IEventBus _eventBus;
		private CUser _user;
		
		private IParticleTargetPart[] _particleTargetParts;
		
		[Inject]
		private void Inject(IEventBus eventBus, CUser user)
		{
			_eventBus = eventBus;
			_user = user;
		}

		public override void Construct()
		{
			base.Construct();
			_maximumCapacity = GetComponent<ITopBarItemWithMaxCapacity>();
			_particleTargetParts = GetComponentsInChildren<IParticleTargetPart>(true);	
		}

		public override void Initialize()
		{
			base.Initialize();
			
			CAnimatedCurrency currency = _user.AnimatedCurrencies.GetCurrency((EValuable)Id);
			currency.ValueChanged += SetValue;
			SetValue(currency.Value);
			_eventBus.Subscribe<CCurrencyParticleStepFinishedSignal>(OnParticleStepFinished);
		}

		private void OnParticleStepFinished(CCurrencyParticleStepFinishedSignal signal)
		{
			if ((ETopBarItem)signal.Diff.Id != Id)
				return;
            
			foreach (IParticleTargetPart targetPart in _particleTargetParts)
			{
				targetPart.ParticleStepFinished(signal.Diff);
			}
		}

		private void SetValue(int value)
		{
			if (_maximumCapacity != null)
			{
				int maxCapacity = _maximumCapacity.GetMaxCapacity();
				_text.SetValue($"{value} / {maxCapacity}");
				return;
			}
            
			_text.SetValue(value, new CNumberFormatter());
		}
	}
}