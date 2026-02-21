// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;

namespace Pharaoh
{
	[Flags]
	public enum EEditorSkips
	{
		None = 0,
		Temp = 1,
		All = ~0
	}
}