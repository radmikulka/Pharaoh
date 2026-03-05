// =========================================
// AUTHOR: Marek Karaba
// DATE:   13.11.2025
// =========================================

using AldaEngine;
using AldaEngine.AldaFramework;
using ServerData;
using TycoonBuilder.Configs;
using UnityEngine;
using Zenject;

namespace TycoonBuilder
{
	public class CUiGainValuableComponent : CUiValuableComponent<IValuable>, IAldaFrameworkComponent
	{
		[SerializeField] private CUiComponentText _countText;
		
		private CUiGlobalsConfig _globalsConfig;

		[Inject]
		private void Inject(CUiGlobalsConfig globalsConfig)
		{
			_globalsConfig = globalsConfig;
		}
		
		protected override void SetValue(IValuable value)
		{
			switch (value)
			{
				case CConsumableValuable consumableValuable:
					_countText.SetValue(consumableValuable.Value, new CPlusSignNumberFormatter());
					break;
				case CResourceValuable resourceValuable:
					_countText.SetValue(resourceValuable.Resource.Amount, new CPlusSignNumberFormatter());
					break;
			}

			_countText.SetColor(_globalsConfig.GainCurrencyColor, false);
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return valuable is CConsumableValuable or CResourceValuable;
		}
	}
}