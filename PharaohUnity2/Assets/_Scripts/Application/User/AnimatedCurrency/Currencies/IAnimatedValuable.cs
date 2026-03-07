// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;
using ServerData;

namespace Pharaoh
{
	public interface IAnimatedValuable
	{
		event Action<SValueChangeArgs> ValueChanged;
		int Value { get; }
	}
}