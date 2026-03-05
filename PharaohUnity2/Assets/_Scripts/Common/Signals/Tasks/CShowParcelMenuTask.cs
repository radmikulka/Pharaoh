// =========================================
// AUTHOR: Marek Karaba
// DATE:   12.09.2025
// =========================================

namespace TycoonBuilder
{
	public class CShowParcelMenuTask
	{
		public readonly int Index;
		public readonly bool IsUnlocked;

		public CShowParcelMenuTask(int index, bool isUnlocked)
		{
			Index = index;
			IsUnlocked = isUnlocked;
		}
	}
}