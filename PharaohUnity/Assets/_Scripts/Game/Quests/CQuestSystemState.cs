using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
	public class CActiveQuestState
	{
		public int QuestIndex;
		public CQuestRuntimeData Data = new();
		/// <summary>Progress >= target — waiting for player to click claim.</summary>
		public bool IsClaimable;
		public bool IsClaimed;
	}

	public class CQuestChapterRuntimeState
	{
		public int ChapterIndex;
		public int TotalClaimedCount;
		public HashSet<int> ClaimedMilestones = new();
		public List<CActiveQuestState> ActiveSlots = new();
		public int NextQuestToActivate;
	}

	public class CMissionQuestSystemState
	{
		public EMissionId Mission;
		public int ActiveChapterIndex;
		public CQuestChapterRuntimeState ChapterState;
		// TODO: serialisation for save system — add in the future
	}
}
