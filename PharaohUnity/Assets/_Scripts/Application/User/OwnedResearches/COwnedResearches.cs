// =========================================
// DATE:   02.03.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class COwnedResearches : CBaseUserComponent
	{
		private readonly Dictionary<EMissionId, HashSet<EResearchId>> _purchased = new();

		private readonly IEventBus _eventBus;
		private readonly ISaveManager _saveManager;

		public COwnedResearches(IEventBus eventBus, ISaveManager saveManager)
		{
			_eventBus = eventBus;
			_saveManager = saveManager;
		}

		public void InitialSync()
		{
			_purchased.Clear();

			CSaveData data = _saveManager.Data;
			foreach (KeyValuePair<int, CMissionResearchSaveData> pair in data.MissionResearch)
			{
				EMissionId mission = (EMissionId)pair.Key;
				HashSet<EResearchId> set = GetOrCreateSet(mission);
				foreach (int id in pair.Value.PurchasedResearchIds)
				{
					set.Add((EResearchId)id);
				}
			}
		}

		public bool HasResearch(EMissionId mission, EResearchId research)
		{
			return _purchased.TryGetValue(mission, out HashSet<EResearchId> set) && set.Contains(research);
		}

		public void Purchase(EMissionId mission, EResearchId research)
		{
			GetOrCreateSet(mission).Add(research);

			PersistToSave(mission);

			_eventBus.Send(new CResearchPurchasedSignal(mission, research));
		}

		private void PersistToSave(EMissionId mission)
		{
			CSaveData data = _saveManager.Data;
			int missionKey = (int)mission;

			if (!data.MissionResearch.TryGetValue(missionKey, out CMissionResearchSaveData saveEntry))
			{
				saveEntry = new CMissionResearchSaveData();
				data.MissionResearch[missionKey] = saveEntry;
			}

			HashSet<EResearchId> set = GetOrCreateSet(mission);
			int[] ids = new int[set.Count];
			int i = 0;
			foreach (EResearchId id in set)
				ids[i++] = (int)id;
			saveEntry.PurchasedResearchIds = ids;
		}

		private HashSet<EResearchId> GetOrCreateSet(EMissionId mission)
		{
			if (_purchased.TryGetValue(mission, out HashSet<EResearchId> existing))
				return existing;

			HashSet<EResearchId> newSet = new();
			_purchased[mission] = newSet;
			return newSet;
		}
	}
}
