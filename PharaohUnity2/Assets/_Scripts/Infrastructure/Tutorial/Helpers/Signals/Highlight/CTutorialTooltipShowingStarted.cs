// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.10.2024
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CTutorialTooltipShowingStarted : IEventBusSignal
	{
		public readonly string LangKey;

		public CTutorialTooltipShowingStarted(string langKey)
		{
			LangKey = langKey;
		}
	}
}