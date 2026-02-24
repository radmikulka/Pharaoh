using System;
using System.Collections.Generic;
using ServerData;
using UnityEngine;

namespace Pharaoh.Building
{
	public class CBuildingTickContext
	{
		private readonly Dictionary<EResource, int> _snapshot = new();
		private readonly Dictionary<EResource, int> _consumed = new();
		private readonly Dictionary<EResource, int> _produced = new();

		public CBuildingTickContext(COwnedResources ownedResources, EMissionId mission)
		{
			foreach (EResource resource in (EResource[]) Enum.GetValues(typeof(EResource)))
			{
				if (resource == EResource.None)
					continue;

				int amount = ownedResources.GetAmount(mission, resource);
				_snapshot[resource] = amount;
			}
		}

		public void ProcessBuilding(SBuildingLevelData levelData)
		{
			float ratio = 1f;

			if (levelData.Upkeep != null && levelData.Upkeep.Length > 0)
			{
				foreach (SResourceAmount upkeep in levelData.Upkeep)
				{
					if (upkeep.Amount <= 0)
						continue;

					int available = GetAvailable(upkeep.Resource);
					float resourceRatio = (float) available / upkeep.Amount;
					ratio = Mathf.Min(ratio, resourceRatio);
				}

				if (ratio <= 0f)
					return;

				foreach (SResourceAmount upkeep in levelData.Upkeep)
				{
					if (upkeep.Amount <= 0)
						continue;

					int consumed = Mathf.FloorToInt(upkeep.Amount * ratio);
					AddTo(_consumed, upkeep.Resource, consumed);
				}
			}

			if (levelData.Production != null && levelData.Production.Length > 0)
			{
				foreach (SResourceAmount production in levelData.Production)
				{
					if (production.Amount <= 0)
						continue;

					int produced = Mathf.FloorToInt(production.Amount * ratio);
					AddTo(_produced, production.Resource, produced);
				}
			}
		}

		public void Apply(COwnedResources ownedResources, EMissionId mission)
		{
			foreach (KeyValuePair<EResource, int> pair in _consumed)
			{
				if (pair.Value > 0)
					ownedResources.Remove(mission, pair.Key, pair.Value);
			}

			foreach (KeyValuePair<EResource, int> pair in _produced)
			{
				if (pair.Value > 0)
					ownedResources.Add(mission, pair.Key, pair.Value);
			}
		}

		private int GetAvailable(EResource resource)
		{
			int snapshot = _snapshot.TryGetValue(resource, out int s) ? s : 0;
			int consumed = _consumed.TryGetValue(resource, out int c) ? c : 0;
			return Mathf.Max(0, snapshot - consumed);
		}

		private static void AddTo(Dictionary<EResource, int> dict, EResource resource, int amount)
		{
			if (dict.ContainsKey(resource))
				dict[resource] += amount;
			else
				dict[resource] = amount;
		}
	}
}
