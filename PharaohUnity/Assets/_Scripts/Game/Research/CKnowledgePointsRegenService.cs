// =========================================
// DATE:   02.03.2026
// =========================================

using System;
using System.Threading;
using AldaEngine;
using AldaEngine.AldaFramework;
using Cysharp.Threading.Tasks;
using ServerData;
using Zenject;

namespace Pharaoh
{
	public class CKnowledgePointsRegenService : IInitializable, IDisposable
	{
		private const int CheckIntervalSeconds = 30;

		private readonly IMissionController _missionController;
		private readonly COwnedResources _ownedResources;
		private readonly CResourceConfigs _resourceConfigs;
		private readonly ISaveManager _saveManager;
		private readonly IEventBus _eventBus;
		private readonly ICtsProvider _ctsProvider;

		private Guid _resourceChangedSub;
		private CancellationTokenSource _regenCts;
		private bool _regenRunning;

		public CKnowledgePointsRegenService(
			IMissionController missionController,
			COwnedResources ownedResources,
			CResourceConfigs resourceConfigs,
			ISaveManager saveManager,
			IEventBus eventBus,
			ICtsProvider ctsProvider)
		{
			_missionController = missionController;
			_ownedResources = ownedResources;
			_resourceConfigs = resourceConfigs;
			_saveManager = saveManager;
			_eventBus = eventBus;
			_ctsProvider = ctsProvider;
		}

		public void Initialize()
		{
			RestoreFromSave();
			_resourceChangedSub = _eventBus.Subscribe<COwnedResourceChangedSignal>(OnResourceChanged);
		}

		public void Dispose()
		{
			StopRegenLoop();
		}

		// ───────────────────────────────── Restore / Offline Catch-up ────

		private void RestoreFromSave()
		{
			EMissionId missionId = _missionController.ActiveMissionId;
			CGameplayConfig config = _resourceConfigs.Gameplay;
			if (config == null)
				return;

			int maxKP = config.MaxKnowledgePoints;
			long intervalMs = (long)config.KnowledgePointsRegenIntervalMinutes * 60 * 1000;

			CMissionResearchSaveData saveData = GetOrCreateSaveEntry(missionId);

			int currentKP = saveData.CurrentKP;
			long nextRegen = saveData.NextKpRegenTimestamp;

			if (currentKP < maxKP && nextRegen > 0)
			{
				long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				long elapsed = now - nextRegen;
				if (elapsed >= 0)
				{
					int earned = (int)(elapsed / intervalMs) + 1;
					int canEarn = maxKP - currentKP;
					earned = Math.Min(earned, canEarn);

					currentKP += earned;
					nextRegen += earned * intervalMs;
				}
			}
			else if (nextRegen == 0 && currentKP < maxKP)
			{
				// Initialize timer
				nextRegen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + intervalMs;
			}

			// Set the KP resource
			int existing = _ownedResources.GetAmount(missionId, EResource.KnowledgePoints);
			int delta = currentKP - existing;
			if (delta != 0)
				_ownedResources.Add(missionId, EResource.KnowledgePoints, delta);

			// Persist restored state
			saveData.CurrentKP = currentKP;
			saveData.NextKpRegenTimestamp = nextRegen;

			if (currentKP < maxKP)
				StartRegenLoop(nextRegen);
		}

		// ───────────────────────────────────────── Regen Loop ────

		private void StartRegenLoop(long nextRegenTimestamp)
		{
			if (_regenRunning)
				return;

			_regenCts = new CancellationTokenSource();
			_regenRunning = true;
			RegenLoopAsync(nextRegenTimestamp, _regenCts.Token).Forget();
		}

		private void StopRegenLoop()
		{
			_regenCts?.Cancel();
			_regenCts?.Dispose();
			_regenCts = null;
			_regenRunning = false;
		}

		private async UniTaskVoid RegenLoopAsync(long nextRegenTimestamp, CancellationToken ct)
		{
			CGameplayConfig config = _resourceConfigs.Gameplay;
			if (config == null)
				return;

			int maxKP = config.MaxKnowledgePoints;
			long intervalMs = (long)config.KnowledgePointsRegenIntervalMinutes * 60 * 1000;
			EMissionId missionId = _missionController.ActiveMissionId;

			while (!ct.IsCancellationRequested)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), cancellationToken: ct)
					.SuppressCancellationThrow();

				if (ct.IsCancellationRequested)
					break;

				long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				if (now < nextRegenTimestamp)
					continue;

				int currentKP = _ownedResources.GetAmount(missionId, EResource.KnowledgePoints);
				if (currentKP >= maxKP)
				{
					_regenRunning = false;
					break;
				}

				_ownedResources.Add(missionId, EResource.KnowledgePoints, 1);
				nextRegenTimestamp += intervalMs;
				currentKP++;

				CMissionResearchSaveData saveData = GetOrCreateSaveEntry(missionId);
				saveData.CurrentKP = currentKP;
				saveData.NextKpRegenTimestamp = nextRegenTimestamp;
				_saveManager.SaveAsync(_ctsProvider.Token).Forget();

				if (currentKP >= maxKP)
				{
					_regenRunning = false;
					break;
				}
			}
		}

		// ───────────────────────────────────── Event Handlers ────

		private void OnResourceChanged(COwnedResourceChangedSignal signal)
		{
			if (signal.Resource.Id != EResource.KnowledgePoints)
				return;

			EMissionId missionId = _missionController.ActiveMissionId;
			if (signal.Mission != missionId)
				return;

			CGameplayConfig config = _resourceConfigs.Gameplay;
			if (config == null)
				return;

			int currentKP = _ownedResources.GetAmount(missionId, EResource.KnowledgePoints);
			int maxKP = config.MaxKnowledgePoints;

			CMissionResearchSaveData saveData = GetOrCreateSaveEntry(missionId);
			saveData.CurrentKP = currentKP;

			if (currentKP < maxKP && !_regenRunning)
			{
				long intervalMs = (long)config.KnowledgePointsRegenIntervalMinutes * 60 * 1000;
				long nextRegen = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + intervalMs;
				saveData.NextKpRegenTimestamp = nextRegen;
				StartRegenLoop(nextRegen);
			}
		}

		// ───────────────────────────────────────── Helpers ────

		private CMissionResearchSaveData GetOrCreateSaveEntry(EMissionId missionId)
		{
			CSaveData data = _saveManager.Data;
			int key = (int)missionId;
			if (!data.MissionResearch.TryGetValue(key, out CMissionResearchSaveData entry))
			{
				entry = new CMissionResearchSaveData();
				data.MissionResearch[key] = entry;
			}
			return entry;
		}
	}
}
