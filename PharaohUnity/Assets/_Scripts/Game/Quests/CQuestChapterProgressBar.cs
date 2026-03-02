using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pharaoh
{
	public class CQuestChapterProgressBar : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _chapterNameLabel;
		[SerializeField] private Slider _progressBar;
		[SerializeField] private Image[] _milestoneIcons;

		public void Refresh(CQuestChapterConfig config, CQuestChapterRuntimeState state)
		{
			if (config == null || state == null) return;

			_chapterNameLabel.text = config.ChapterName;
			_progressBar.value = config.Quests is { Length: > 0 }
				? Mathf.Clamp01((float)state.TotalClaimedCount / config.Quests.Length)
				: 0f;

			RefreshMilestoneIcons(config, state);
		}

		private void RefreshMilestoneIcons(CQuestChapterConfig config, CQuestChapterRuntimeState state)
		{
			if (_milestoneIcons == null) return;

			int numMilestones = config.MilestoneRewards?.Length ?? 0;

			for (int i = 0; i < _milestoneIcons.Length; i++)
			{
				if (_milestoneIcons[i] == null) continue;

				bool hasMilestone = i < numMilestones;
				_milestoneIcons[i].gameObject.SetActive(hasMilestone);

				if (hasMilestone)
				{
					bool claimed = state.ClaimedMilestones.Contains(i);
					_milestoneIcons[i].color = claimed ? Color.yellow : Color.white;
				}
			}
		}
	}
}
