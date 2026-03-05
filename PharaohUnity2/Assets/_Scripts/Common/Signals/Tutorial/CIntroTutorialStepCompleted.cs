// =========================================
// AUTHOR: Radek Mikulka
// DATE:   22.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CIntroTutorialStepCompleted : IEventBusSignal
	{
		public readonly EIntroTutorialStep Step;
		
		public CIntroTutorialStepCompleted(EIntroTutorialStep step)
		{
			Step = step;
		}
	}
}