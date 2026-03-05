// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.09.2025
// =========================================

using System.Collections.Generic;
using AldaEngine;

namespace ServerData
{
	public class CFactoryProduct
	{
		private readonly List<SResource> _reqirements = new();
		
		public IReadOnlyList<SResource> Requirements => _reqirements;
		public SResource Resource { get; private set; }
		public long ProductionTimeInMs {get; private set;}
		public long ProductionTimeInSeconds => ProductionTimeInMs / CTimeConst.Second.InMilliseconds;

		public CFactoryProduct(EResource resource)
		{
			Resource = new SResource(resource, 0);
		}

		public void AddProductionTime(long timeInMs)
		{
			ProductionTimeInMs += timeInMs;
		}

		public void AddProducedAmount(int amount)
		{
			Resource = new SResource(Resource.Id, Resource.Amount + amount);
		}

		public void AddRequirement(SResource resource)
		{
			int requirementIndex = GetResourceIndex(resource.Id);
			if (requirementIndex < 0)
			{
				_reqirements.Add(resource);
				return;
			}

			int currentAmount = _reqirements[requirementIndex].Amount;
			_reqirements[requirementIndex] = new SResource(resource.Id, currentAmount + resource.Amount);
		}

		private int GetResourceIndex(EResource id)
		{
			for (int i = 0; i < _reqirements.Count; i++)
			{
				SResource reqirement = _reqirements[i];
				if (reqirement.Id == id)
				{
					return i;
				}
			}

			return -1;
		}
	}
}