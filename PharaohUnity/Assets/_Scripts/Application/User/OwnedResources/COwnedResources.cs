// =========================================
// DATE:   22.02.2026
// =========================================

using System.Collections.Generic;
using AldaEngine;
using ServerData;

namespace Pharaoh
{
	public class COwnedResources : CBaseUserComponent
	{
		private readonly Dictionary<EMissionId, CMissionResources> _missionResources = new();
		private readonly COwnedResource _sharedGold = new(EResource.Gold);

		private readonly IEventBus _eventBus;
		private readonly CResourceConfigs _resourceConfigs;

		public COwnedResources(IEventBus eventBus, CResourceConfigs resourceConfigs)
		{
			_eventBus = eventBus;
			_resourceConfigs = resourceConfigs;
		}

		public void Add(EMissionId mission, EResource resource, int amount, CValueModifyParams modifyParams = null)
		{
			COwnedResource owned = GetOwnedResource(mission, resource);
			SValueChangeArgs args = owned.Modify(amount, modifyParams);
			_eventBus.Send(new COwnedResourceChangedSignal(new SResource(resource, amount), mission, args));
		}

		public void Remove(EMissionId mission, EResource resource, int amount, CValueModifyParams modifyParams = null)
		{
			Add(mission, resource, -amount, modifyParams);
		}

		public bool HasEnough(EMissionId mission, EResource resource, int amount)
		{
			COwnedResource owned = GetOwnedResource(mission, resource);
			return owned.HasEnough(amount);
		}

		public bool HasEnough(EMissionId mission, SResource[] costs)
		{
			foreach (SResource cost in costs)
			{
				if (!HasEnough(mission, cost.Id, cost.Amount))
					return false;
			}

			return true;
		}

		public int GetAmount(EMissionId mission, EResource resource)
		{
			COwnedResource owned = GetOwnedResource(mission, resource);
			return owned.Amount;
		}

		public COwnedResource GetOwnedResource(EMissionId mission, EResource resource)
		{
			if (resource == EResource.Gold)
				return _sharedGold;

			CMissionResources missionResources = GetOrCreateMissionResources(mission);
			return missionResources.GetResource(resource);
		}

		private CMissionResources GetOrCreateMissionResources(EMissionId mission)
		{
			if (_missionResources.TryGetValue(mission, out CMissionResources existing))
				return existing;

			CMissionConfig config = _resourceConfigs.Missions.GetConfig(mission);
			EResource[] availableResources = config != null ? config.AvailableResources : System.Array.Empty<EResource>();
			CMissionResources missionResources = new(availableResources, _sharedGold);
			_missionResources[mission] = missionResources;
			return missionResources;
		}

		public override void Dispose()
		{
			base.Dispose();

			foreach (KeyValuePair<EMissionId, CMissionResources> pair in _missionResources)
			{
				pair.Value.Dispose();
			}

			_sharedGold.Dispose();
		}
	}
}
