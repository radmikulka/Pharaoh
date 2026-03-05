// =========================================
// AUTHOR: Radek Mikulka
// DATE:   28.10.2024
// =========================================

namespace TycoonBuilder
{
	public class CShowScreenTask
	{
		public readonly EScreenId ScreenId;
		public readonly bool ClosePreviousScreen;

		public CShowScreenTask(EScreenId screenId)
		{
			ScreenId = screenId;
			ClosePreviousScreen = false;
		}

		public CShowScreenTask(EScreenId screenId, bool closePreviousScreen)
		{
			ScreenId = screenId;
			ClosePreviousScreen = closePreviousScreen;
		}
	}
}