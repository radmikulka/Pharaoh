// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;
using ServerData;

namespace Pharaoh
{
	public interface IAnimatedCurrency
	{
		event Action<SValueChangeArgs> ValueChanged;
		int Value { get; }
		public void Tick(long timestamp);
	}
}