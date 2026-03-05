// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.09.2025
// =========================================

using System;

namespace TycoonBuilder
{
	[Flags]
	public enum EEditorSkips
	{
		None = 0,
		Dialogues = 1 << 0,
		Popups = 1 << 1,
		IntroCutscene = 1 << 2,
		ContractCutscenes = 1 << 3,
		ContractAnimation = 1 << 4,
		VehicleShowcase = 1 << 5,
		All = ~0
	}
}