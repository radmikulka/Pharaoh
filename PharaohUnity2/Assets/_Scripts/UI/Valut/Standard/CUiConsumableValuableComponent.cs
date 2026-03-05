// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.12.2024
// =========================================

using System;
using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiConsumableValuableComponent : CUiValuableComponent<IValuable>, IConstructable, IScreenCloseEnd
	{
		[SerializeField] private CUiComponentText _countText;

		private IValuable _activeValuable;
		private Guid _valueChangeSubscriptionId;
		private CUiValuablePrice _parentPrice;
		private CUiGlobalsConfig _uiGlobals;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(CUiGlobalsConfig uiGlobals, IEventBus eventBus)
		{
			_uiGlobals = uiGlobals;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_parentPrice = GetComponentInParent<CUiValuablePrice>(true);
		}
		
		protected override void SetValue(IValuable value)
		{
			_activeValuable = value;

			if (value is CConsumableValuable consumable)
			{
				_countText.SetValue(consumable.Value, new CNumberFormatter());
			}
			
			if(value is CEventCoinValuable eventCoin)
			{
				_countText.SetValue(eventCoin.Amount, new CNumberFormatter());
			}
			
			
			if (_parentPrice)
			{
				BindToValueChange();
				RepaintTextColor();
			}
		}
		
		private void BindToValueChange()
		{
			TryUnsubscribeValueChange();
			_valueChangeSubscriptionId = _eventBus.Subscribe<COwnedValuableChangedSignal>(OnOwnedValuableChanged);
		}

		private void OnOwnedValuableChanged(COwnedValuableChangedSignal signal)
		{
			if(signal.Valuable.Id != _activeValuable.Id)
				return;
			
			RepaintTextColor();
		}
		
		private void RepaintTextColor()
		{
			if(_parentPrice == null)
				return;
			_countText.SetColor(_parentPrice.CanAfford() ? _uiGlobals.EnoughCurrencyColor : _uiGlobals.NotEnoughCurrencyColor, false);
		}
		
		public void OnScreenCloseEnd()
		{
			TryUnsubscribeValueChange();
		}
		
		private void TryUnsubscribeValueChange()
		{
			_eventBus.Unsubscribe(_valueChangeSubscriptionId);
		}

		protected override bool IsValidValuable(IValuable valut)
		{
			return valut is CConsumableValuable or CEventCoinValuable;
		}
	}
}