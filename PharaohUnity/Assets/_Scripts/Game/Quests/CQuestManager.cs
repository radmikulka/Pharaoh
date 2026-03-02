using System;
using System.Collections.Generic;
using AldaEngine;
using AldaEngine.AldaFramework;
using Pharaoh.Building;
using ServerData;

namespace Pharaoh
{
	public class CQuestManager : IInitializable
	{
		private const int MaxActiveSlots = 3;

		private readonly IMissionController _missionController;
		private readonly CDesignMissionsConfigs _missionConfigs;
		private readonly COwnedResources _ownedResources;
		private readonly CBuildingManager _buildingManager;
		private readonly IEventBus _eventBus;

		private CMissionQuestSystemState _state;

		public CQuestManager(
			IMissionController missionController,
			CDesignMissionsConfigs missionConfigs,
			COwnedResources ownedResources,
			CBuildingManager buildingManager,
			IEventBus eventBus)
		{
			_missionController = missionController;
			_missionConfigs = missionConfigs;
			_ownedResources = ownedResources;
			_buildingManager = buildingManager;
			_eventBus = eventBus;
		}

		public void Initialize()
		{
			EMissionId missionId = _missionController.ActiveMissionId;

			_state = new CMissionQuestSystemState
			{
				Mission = missionId,
				ActiveChapterIndex = 0,
				ChapterState = CreateChapterState(0)
			};

			ActivateNextSlots();

			_eventBus.Subscribe<COwnedResourceChangedSignal>(OnResourceChanged);
			_eventBus.Subscribe<CBuildingUpgradedSignal>(OnBuildingUpgraded);
			_eventBus.Subscribe<CBuildingPlacedSignal>(OnBuildingPlaced);
		}

		// ─────────────────────────────────────────── Public API ────

		public IReadOnlyList<CActiveQuestState> GetActiveSlots()
			=> _state?.ChapterState.ActiveSlots ?? (IReadOnlyList<CActiveQuestState>)System.Array.Empty<CActiveQuestState>();

		public CQuestChapterConfig GetCurrentChapterConfig()
		{
			if (_state == null) return null;
			CMissionData data = _missionConfigs.GetMission(_state.Mission);
			int chapterIdx = _state.ActiveChapterIndex;
			if (data?.QuestChapters == null || chapterIdx >= data.QuestChapters.Length) return null;
			return data.QuestChapters[chapterIdx];
		}

		/// <summary>Normalised progress 0..1 (claimed quests / total quests in chapter).</summary>
		public float GetChapterProgress()
		{
			CQuestChapterConfig config = GetCurrentChapterConfig();
			if (config?.Quests == null || config.Quests.Length == 0) return 0f;
			return (float)_state.ChapterState.TotalClaimedCount / config.Quests.Length;
		}

		public int GetClaimedQuestCount() => _state?.ChapterState.TotalClaimedCount ?? 0;

		public IReadOnlyCollection<int> GetClaimedMilestones()
			=> _state?.ChapterState.ClaimedMilestones ?? (IReadOnlyCollection<int>)Array.Empty<int>();

		/// <summary>Current progress for a slot (works for both cumulative and snapshot quest types).</summary>
		public float GetSlotProgress(int slotIndex)
		{
			if (_state == null) return 0f;
			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;
			if (slotIndex < 0 || slotIndex >= slots.Count) return 0f;
			CQuestChapterConfig chapter = GetCurrentChapterConfig();
			if (chapter?.Quests == null) return 0f;
			CActiveQuestState slot = slots[slotIndex];
			return chapter.Quests[slot.QuestIndex].GetCurrentProgress(slot.Data, _buildingManager.GetBuildingCount);
		}

		/// <summary>Called when the player taps the claim button on a completed quest slot.</summary>
		public void ClaimQuest(int slotIndex)
		{
			if (_state == null) return;
			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;

			if (slotIndex < 0 || slotIndex >= slots.Count) return;
			CActiveQuestState slot = slots[slotIndex];

			if (!slot.IsClaimable || slot.IsClaimed) return;

			// 1. Grant quest reward
			CQuestChapterConfig chapterConfig = GetCurrentChapterConfig();
			CQuestConfigBase questConfig = chapterConfig.Quests[slot.QuestIndex];

			if (questConfig.Reward != null)
			{
				foreach (SResource reward in questConfig.Reward)
					_ownedResources.Add(_state.Mission, reward.Id, reward.Amount);
			}

			slot.IsClaimed = true;
			_state.ChapterState.TotalClaimedCount++;

			// 2. Check milestones
			CheckAndGrantMilestones(chapterConfig);

			// 3. Activate next quest or clear the slot
			bool chapterDone = ActivateNextQuestInSlot(slotIndex, chapterConfig);

			// 4. Notify
			_eventBus.Send(new CQuestClaimedSignal(slotIndex));

			// 5. Chapter transition
			if (chapterDone)
			{
				int completedChapter = _state.ActiveChapterIndex;
				_eventBus.Send(new CChapterCompletedSignal(completedChapter));

				int nextChapter = completedChapter + 1;
				CMissionData missionData = _missionConfigs.GetMission(_state.Mission);
				if (missionData?.QuestChapters != null && nextChapter < missionData.QuestChapters.Length)
				{
					_state.ActiveChapterIndex = nextChapter;
					_state.ChapterState = CreateChapterState(nextChapter);
					ActivateNextSlots();
				}
			}
		}

		// ─────────────────────────────────────────── Event Handlers ────

		private void OnResourceChanged(COwnedResourceChangedSignal signal)
		{
			if (_state == null || signal.Mission != _state.Mission) return;

			float delta = signal.ChangeArgs.Difference;
			EResource resource = signal.Resource.Id;
			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;
			CQuestChapterConfig chapter = GetCurrentChapterConfig();

			for (int i = 0; i < slots.Count; i++)
			{
				CActiveQuestState slot = slots[i];
				if (slot.IsClaimed || slot.IsClaimable) continue;

				CQuestConfigBase config = chapter.Quests[slot.QuestIndex];
				config.OnResourceChanged(resource, delta, slot.Data);
				CheckCompletion(slot, config, i);
			}
		}

		private void OnBuildingUpgraded(CBuildingUpgradedSignal signal)
		{
			if (_state == null) return;

			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;
			CQuestChapterConfig chapter = GetCurrentChapterConfig();

			for (int i = 0; i < slots.Count; i++)
			{
				CActiveQuestState slot = slots[i];
				if (slot.IsClaimed || slot.IsClaimable) continue;

				CQuestConfigBase config = chapter.Quests[slot.QuestIndex];
				config.OnBuildingUpgraded(signal.BuildingId, signal.NewLevel, slot.Data);
				CheckCompletion(slot, config, i);
			}
		}

		private void OnBuildingPlaced(CBuildingPlacedSignal signal)
		{
			if (_state == null) return;

			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;
			CQuestChapterConfig chapter = GetCurrentChapterConfig();

			for (int i = 0; i < slots.Count; i++)
			{
				CActiveQuestState slot = slots[i];
				if (slot.IsClaimed || slot.IsClaimable) continue;

				CQuestConfigBase config = chapter.Quests[slot.QuestIndex];
				config.OnBuildingPlaced(signal.BuildingId, slot.Data);

				// Snapshot quests (CHaveBuildingsQuestConfig) re-evaluate on each event
				CheckCompletion(slot, config, i);
			}
		}

		// ─────────────────────────────────────────── Helpers ────

		private CQuestChapterRuntimeState CreateChapterState(int chapterIndex)
		{
			return new CQuestChapterRuntimeState
			{
				ChapterIndex = chapterIndex,
				TotalClaimedCount = 0,
				NextQuestToActivate = 0
			};
		}

		private void ActivateNextSlots()
		{
			CQuestChapterConfig config = GetCurrentChapterConfig();
			if (config?.Quests == null) return;

			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;

			while (slots.Count < MaxActiveSlots && _state.ChapterState.NextQuestToActivate < config.Quests.Length)
			{
				slots.Add(new CActiveQuestState
				{
					QuestIndex = _state.ChapterState.NextQuestToActivate
				});
				_state.ChapterState.NextQuestToActivate++;
			}
		}

		private void CheckCompletion(CActiveQuestState slot, CQuestConfigBase config, int slotIndex)
		{
			if (slot.IsClaimable) return;

			float progress = config.GetCurrentProgress(slot.Data, _buildingManager.GetBuildingCount);
			_eventBus.Send(new CQuestProgressUpdatedSignal(slotIndex, progress, config.TargetValue));

			if (config.IsCompleted(slot.Data, _buildingManager.GetBuildingCount))
			{
				slot.IsClaimable = true;
				_eventBus.Send(new CQuestCompletedSignal(slotIndex));
			}
		}

		private void CheckAndGrantMilestones(CQuestChapterConfig config)
		{
			if (config.MilestoneRewards == null) return;

			CQuestChapterRuntimeState chapterState = _state.ChapterState;

			for (int i = 0; i < config.MilestoneRewards.Length; i++)
			{
				if (chapterState.ClaimedMilestones.Contains(i)) continue;

				int threshold = config.GetMilestoneThreshold(i);
				if (chapterState.TotalClaimedCount >= threshold)
				{
					chapterState.ClaimedMilestones.Add(i);

					SResource[] reward = config.MilestoneRewards[i];
					if (reward != null)
					{
						foreach (SResource r in reward)
							_ownedResources.Add(_state.Mission, r.Id, r.Amount);
					}

					_eventBus.Send(new CMilestoneReachedSignal(chapterState.ChapterIndex, i, reward));
				}
			}
		}

		/// <summary>
		/// Replaces a claimed slot with the next available quest.
		/// Returns true when the chapter has no more active quests (chapter complete).
		/// </summary>
		private bool ActivateNextQuestInSlot(int slotIndex, CQuestChapterConfig config)
		{
			List<CActiveQuestState> slots = _state.ChapterState.ActiveSlots;

			if (_state.ChapterState.NextQuestToActivate < config.Quests.Length)
			{
				slots[slotIndex] = new CActiveQuestState
				{
					QuestIndex = _state.ChapterState.NextQuestToActivate
				};
				_state.ChapterState.NextQuestToActivate++;
				return false;
			}

			// No more quests to queue — remove the slot
			slots.RemoveAt(slotIndex);
			return slots.Count == 0;
		}
	}
}
