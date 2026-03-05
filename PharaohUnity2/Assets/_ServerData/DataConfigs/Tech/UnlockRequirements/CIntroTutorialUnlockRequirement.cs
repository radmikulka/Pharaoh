// =========================================
// AUTHOR: Radek Mikulka
// DATE:   01.10.2025
// =========================================

namespace ServerData
{
	public class CIntroTutorialUnlockRequirement : IUnlockRequirement
	{
		public readonly EIntroTutorialStep Step;

		public CIntroTutorialUnlockRequirement(EIntroTutorialStep step)
		{
			Step = step;
		}
	}
}