// =========================================
// AUTHOR: Marek Karaba
// DATE:   20.11.2025
// =========================================

using AldaEngine;

namespace TycoonBuilder
{
	public class CScreenInfoSeenSignal : IEventBusSignal
	{
		public readonly EScreenInfoId InfoScreen;

		public CScreenInfoSeenSignal(EScreenInfoId infoScreen)
		{
			InfoScreen = infoScreen;
		}
	}
}