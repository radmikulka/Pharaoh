// =========================================
// AUTHOR: Radek Mikulka
// DATE:   30.01.2026
// =========================================

using ServerData;

namespace TycoonBuilder
{
	public class CActiveTask
	{
		public readonly CCountableTaskRequirement[] Requirements;
		public readonly IValuable[] Rewards;
		public readonly ETaskId TaskId;
		public readonly string Uid;
		public readonly int FrontendOrder;
		public bool IsClaimed { get; private set; }

		public CActiveTask(string uid, ETaskId taskId, CCountableTaskRequirement[] requirements, IValuable[] rewards, bool isClaimed, int frontendOrder)
		{
			Requirements = requirements;
			Rewards = rewards;
			TaskId = taskId;
			Uid = uid;
			IsClaimed = isClaimed;
			FrontendOrder = frontendOrder;
		}
		
		public void MarkAsClaimed()
		{
			IsClaimed = true;
		}
	}
}