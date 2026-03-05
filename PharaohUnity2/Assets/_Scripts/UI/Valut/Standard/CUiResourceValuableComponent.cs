// =========================================
// AUTHOR: Marek Karaba
// DATE:   25.07.2025
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
	public class CUiResourceValuableComponent : CUiValuableComponent<CResourceValuable>, IConstructable, IScreenCloseEnd
	{
		[SerializeField] private CUiComponentText _countText;

		private CUiGlobalsConfig _uiGlobalsConfig;
		private IEventBus _eventBus;
		
		private Guid _valueChangeSubscriptionId;
		private CResourceValuable _activeValue;
		private CUiValuablePrice _parentPrice;

		[Inject]
		private void Inject(CUiGlobalsConfig uiGlobalsConfig, IEventBus eventBus)
		{
			_uiGlobalsConfig = uiGlobalsConfig;
			_eventBus = eventBus;
		}
		
		public void Construct()
		{
			_parentPrice = GetComponentInParent<CUiValuablePrice>(true);
		}
		
		protected override void SetValue(CResourceValuable value)
		{
			_activeValue = value;
			_countText.SetValue(value.Resource.Amount, new CNumberFormatter());
			
			if (_parentPrice)
			{
				BindToValueChange();
				RepaintTextColor();
			}
		}

		private void BindToValueChange()
		{
			TryUnsubscribeValueChange();
			_valueChangeSubscriptionId = _eventBus.Subscribe<CWarehouseResourceChangedSignal>(OnWarehouseResourceChanged);
		}

		private void OnWarehouseResourceChanged(CWarehouseResourceChangedSignal signal)
		{
			if(signal.NewValue.Id != _activeValue.Resource.Id)
				return;
			
			RepaintTextColor();
		}

		private void RepaintTextColor()
		{
			if(_parentPrice == null)
				return;
			_countText.SetColor(_parentPrice.CanAfford() ? _uiGlobalsConfig.EnoughCurrencyColor : _uiGlobalsConfig.NotEnoughCurrencyColor, false);
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
			return valut is CResourceValuable;
		}
	}
}