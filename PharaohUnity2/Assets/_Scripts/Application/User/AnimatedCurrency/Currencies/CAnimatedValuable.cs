// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;
using ServerData;

namespace Pharaoh
{
	public class CAnimatedValuable : IAnimatedCurrency
	{
		private readonly CConsumableOwnedValuable _valuable;

		public event Action<SValueChangeArgs> ValueChanged;

		public int Value => _valuable.Amount;

		public CAnimatedValuable(CConsumableOwnedValuable valuable)
		{
			_valuable = valuable;
			_valuable.ValueChanged += OnValuableChanged;
		}

		private void OnValuableChanged(SValueChangeArgs args)
		{
			ValueChanged?.Invoke(args);
		}
	}
}