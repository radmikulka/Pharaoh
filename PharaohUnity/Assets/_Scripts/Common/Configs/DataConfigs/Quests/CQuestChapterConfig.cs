using ServerData;

namespace Pharaoh
{
	public class CQuestChapterConfig
	{
		public string ChapterName;
		public CQuestConfigBase[] Quests;

		/// <summary>
		/// Milestone rewards evenly distributed across all quests in the chapter.
		/// Array length = number of milestones.
		/// </summary>
		public SResource[][] MilestoneRewards;

		/// <summary>
		/// Returns the claimed-quest threshold at which milestone[index] is triggered.
		/// Milestones are evenly spaced across total quest count.
		/// </summary>
		public int GetMilestoneThreshold(int milestoneIndex)
		{
			if (MilestoneRewards == null || MilestoneRewards.Length == 0 || Quests == null)
				return int.MaxValue;

			int numMilestones = MilestoneRewards.Length;
			int numQuests = Quests.Length;
			return (int)System.Math.Round((milestoneIndex + 1) * numQuests / (double)numMilestones);
		}
	}
}
