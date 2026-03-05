// =========================================
// AUTHOR: Marek Karaba
// DATE:   15.12.2025
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
	public class CUiEventPointValuableComponent : CUiValuableComponent<CEventPointValuable>, IConstructable
	{
		[SerializeField] private CUiComponentText _countText;

		private CUiGlobalsConfig _uiGlobalsConfig;
		
		private CUiValuablePrice _parentPrice;

		[Inject]
		private void Inject(CUiGlobalsConfig uiGlobalsConfig)
		{
			_uiGlobalsConfig = uiGlobalsConfig;
		}
		
		public void Construct()
		{
			_parentPrice = GetComponentInParent<CUiValuablePrice>(true);
		}
		
		protected override void SetValue(CEventPointValuable value)
		{
			_countText.SetValue(value.Amount, new CNumberFormatter());
			if (_parentPrice)
			{
				RepaintTextColor();
			}
		}

		private void RepaintTextColor()
		{
			if(_parentPrice == null)
				return;
			_countText.SetColor(_parentPrice.CanAfford() ? _uiGlobalsConfig.EnoughCurrencyColor : _uiGlobalsConfig.NotEnoughCurrencyColor, false);
		}

		protected override bool IsValidValuable(IValuable valut)
		{
			return valut is CEventPointValuable;
		}
	}
}