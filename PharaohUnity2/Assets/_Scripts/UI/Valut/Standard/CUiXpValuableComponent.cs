// =========================================
// AUTHOR: Juraj Joscak
// DATE:   19.08.2025
// =========================================

using AldaEngine;
using ServerData;
using UnityEngine;

namespace TycoonBuilder
{
	public class CUiXpValuableComponent : CUiValuableComponent<CXpValuable>
	{
		[SerializeField] private CUiComponentText _countText;
		
		protected override void SetValue(CXpValuable value)
		{
			_countText.SetValue(value.Amount, new CNumberFormatter());
		}

		protected override bool IsValidValuable(IValuable valuable)
		{
			return valuable is CXpValuable;
		}
	}
}