// =========================================
// AUTHOR: Radek Mikulka
// DATE:   08.08.2025
// =========================================

namespace TycoonBuilder
{
	public class CSetActiveLetterboxTask
	{
		public readonly bool State;
		public readonly bool Animated;

		public CSetActiveLetterboxTask(bool state, bool animated)
		{
			Animated = animated;
			State = state;
		}
	}
}