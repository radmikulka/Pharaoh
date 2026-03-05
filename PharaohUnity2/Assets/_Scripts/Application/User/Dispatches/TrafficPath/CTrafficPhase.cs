// =========================================
// AUTHOR: Radek Mikulka
// DATE:   06.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CTrafficPhase : ITrafficPhase
	{
		public long Duration { get; }

		protected CTrafficPhase(long duration)
		{
			Duration = duration;
		}
	}
}