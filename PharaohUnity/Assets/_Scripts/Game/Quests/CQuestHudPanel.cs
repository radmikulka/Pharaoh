using System.Collections.Generic;
using AldaEngine;
using UnityEngine;
using Zenject;

namespace Pharaoh
{
	public class CQuestHudPanel : MonoBehaviour
	{
		[SerializeField] private CQuestChapterProgressBar _chapterProgressBar;
		[SerializeField] private CQuestSlotItem[] _slots;

		private CQuestManager _questManager;
		private IEventBus _eventBus;

		[Inject]
		private void Inject(CQuestManager questManager, IEventBus eventBus)
		{
			_questManager = questManager;
			_eventBus = eventBus;

			_eventBus.Subscribe<CQuestProgressUpdatedSignal>(OnQuestProgressUpdated);
			_eventBus.Subscribe<CQuestCompletedSignal>(OnQuestCompleted);
			_eventBus.Subscribe<CQuestClaimedSignal>(OnQuestClaimed);
			_eventBus.Subscribe<CMilestoneReachedSignal>(OnMilestoneReached);
			_eventBus.Subscribe<CChapterCompletedSignal>(OnChapterCompleted);

			for (int i = 0; i < _slots.Length; i++)
				_slots[i].Initialize(_questManager, i);

			RefreshAll();
		}

		private void OnQuestProgressUpdated(CQuestProgressUpdatedSignal signal) => RefreshSlot(signal.SlotIndex);
		private void OnQuestCompleted(CQuestCompletedSignal signal) => RefreshSlot(signal.SlotIndex);
		private void OnQuestClaimed(CQuestClaimedSignal signal) => RefreshAll();
		private void OnMilestoneReached(CMilestoneReachedSignal signal) => RefreshProgressBar();
		private void OnChapterCompleted(CChapterCompletedSignal signal) => RefreshAll();

		private void RefreshAll()
		{
			RefreshProgressBar();
			IReadOnlyList<CActiveQuestState> activeSlots = _questManager.GetActiveSlots();
			CQuestChapterConfig chapterConfig = _questManager.GetCurrentChapterConfig();

			for (int i = 0; i < _slots.Length; i++)
			{
				CActiveQuestState slotState = i < activeSlots.Count ? activeSlots[i] : null;
				CQuestConfigBase questConfig = slotState != null && chapterConfig?.Quests != null
					? chapterConfig.Quests[slotState.QuestIndex]
					: null;
				_slots[i].Refresh(slotState, questConfig, _questManager.GetSlotProgress(i));
			}
		}

		private void RefreshSlot(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= _slots.Length) return;

			IReadOnlyList<CActiveQuestState> activeSlots = _questManager.GetActiveSlots();
			CQuestChapterConfig chapterConfig = _questManager.GetCurrentChapterConfig();

			CActiveQuestState slotState = slotIndex < activeSlots.Count ? activeSlots[slotIndex] : null;
			CQuestConfigBase questConfig = slotState != null && chapterConfig?.Quests != null
				? chapterConfig.Quests[slotState.QuestIndex]
				: null;

			_slots[slotIndex].Refresh(slotState, questConfig, _questManager.GetSlotProgress(slotIndex));
		}

		private void RefreshProgressBar()
		{
			if (_chapterProgressBar == null) return;

			CQuestChapterConfig config = _questManager.GetCurrentChapterConfig();
			var snapshotState = new CQuestChapterRuntimeState
			{
				TotalClaimedCount = _questManager.GetClaimedQuestCount(),
				ClaimedMilestones = new HashSet<int>(_questManager.GetClaimedMilestones())
			};
			_chapterProgressBar.Refresh(config, snapshotState);
		}
	}
}
