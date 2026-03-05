// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2025
// =========================================

using System;

namespace ServerData
{
	[Flags]
	public enum EMovementType
	{
		None = 1 << 0, 
		Road = 1 << 1,
		Rail = 1 << 2,
		Water = 1 << 3,
		Air = 1 << 4,
		All = Road | Rail | Water | Air
	}
}