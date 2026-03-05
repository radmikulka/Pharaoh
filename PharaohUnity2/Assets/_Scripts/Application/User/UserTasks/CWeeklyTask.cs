// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CWeeklyTask
	{
		public readonly IValuable[] Rewards;
		public readonly int RequiredPoints;
		public bool IsClaimed { get; private set; }
		public readonly string Uid;

		public CWeeklyTask(IValuable[] rewards, int requiredPoints, bool isClaimed, string uid)
		{
			Rewards = rewards;
			RequiredPoints = requiredPoints;
			IsClaimed = isClaimed;
			Uid = uid;
		}

		public void MarkAsClaimed()
		{
			IsClaimed = true;
		}
	}
}