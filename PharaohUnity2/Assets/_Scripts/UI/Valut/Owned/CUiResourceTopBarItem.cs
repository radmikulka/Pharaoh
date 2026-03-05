// =========================================
// AUTHOR: Juraj Joscak
// DATE:   17.02.2026
// =========================================

using AldaEngine;
using KBCore.Refs;
using ServerData;
using TycoonBuilder.Ui;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiResourceTopBarItem : CUiTopBarItem
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

			CAnimatedCurrency currency = _user.AnimatedCurrencies.GetResource((EResource)Id - 1000);
			currency.ValueChanged += SetValue;
			SetValue(currency.Value);
			
			_eventBus.Subscribe<CCurrencyParticleStepFinishedSignal>(OnParticleStepFinished);
		}

		private void OnParticleStepFinished(CCurrencyParticleStepFinishedSignal signal)
		{
			if(signal.Diff.Id != EValuable.Resource)
				return;
			
			CResourceValuable resourceDiff = (CResourceValuable)signal.Diff;
			if ((ETopBarItem)resourceDiff.Resource.Id + 1000 != Id)
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