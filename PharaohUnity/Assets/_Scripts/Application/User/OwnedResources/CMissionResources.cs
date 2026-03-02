// =========================================
// DATE:   22.02.2026
// =========================================

using System.Collections.Generic;
using ServerData;

namespace Pharaoh
{
	public class CMissionResources
	{
		private readonly Dictionary<EResource, COwnedResource> _resources = new();

		public CMissionResources(EResource[] availableResources, COwnedResource sharedGold, CGameplayConfig gameplayConfig = null)
		{
			_resources[EResource.Gold] = sharedGold;

			foreach (EResource resource in availableResources)
			{
				if (resource == EResource.Gold || resource == EResource.None)
					continue;

				if (resource == EResource.KnowledgePoints && gameplayConfig != null)
				{
					int max = gameplayConfig.MaxKnowledgePoints;
					_resources[resource] = new COwnedResource(resource, max, max);
				}
				else
				{
					_resources[resource] = new COwnedResource(resource);
				}
			}
		}

		public COwnedResource GetResource(EResource resource)
		{
			return _resources.TryGetValue(resource, out COwnedResource owned) ? owned : null;
		}

		public bool HasResource(EResource resource)
		{
			return _resources.ContainsKey(resource);
		}

		public IEnumerable<COwnedResource> GetAll()
		{
			return _resources.Values;
		}

		public void Dispose()
		{
			foreach (KeyValuePair<EResource, COwnedResource> pair in _resources)
			{
				if (pair.Key == EResource.Gold)
					continue;

				pair.Value.Dispose();
			}
		}
	}
}
