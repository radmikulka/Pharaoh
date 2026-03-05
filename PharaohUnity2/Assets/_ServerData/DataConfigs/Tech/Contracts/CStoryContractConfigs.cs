// =========================================
// AUTHOR: Radek Mikulka
// DATE:   15.08.2025
// =========================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerData
{
	public class CStoryContractConfigs
	{
		private readonly Dictionary<EStaticContractId, CStoryContractConfig> _configsById = new();
		private readonly Dictionary<ERegion, CStoryContractConfig[]> _configsByRegion = new();

		public IEnumerable<CStoryContractConfig> AllConfigs => _configsById.Values;
		
		public CStoryContractConfig GetConfig(EStaticContractId id)
		{
			if (_configsById.TryGetValue(id, out CStoryContractConfig config))
				return config;
			throw new KeyNotFoundException($"Contract config with id {id} not found.");
		}

		public IEnumerable<CStoryContractConfig> GetContractsUpToYear(EYearMilestone yearMilestone)
		{
			foreach (var config in _configsById)
			{
				bool isUnlocked = config.Value.IsUnlockedInYear(yearMilestone);
				if (!isUnlocked)
					continue;
				yield return config.Value;
			}
		}
		
		public CStoryContractConfig GetConfigOrDefault(EStaticContractId id)
		{
			_configsById.TryGetValue(id, out CStoryContractConfig config);
			return config;
		}

		public bool ConfigExists(EStaticContractId id)
		{
			return _configsById.ContainsKey(id);
		}
		
		public CContractTask GetTask(SStaticContractPointer contractPointer)
		{
			CStoryContractConfig config = GetConfig(contractPointer.Id);
			return config.GetTaskConfig(contractPointer.Task - 1);
		}
		
		public CContractTask GetTaskOrDefault(SStaticContractPointer contractPointer)
		{
			CStoryContractConfig config = GetConfigOrDefault(contractPointer.Id);
			if (config == null)
				return null;

			CContractTask task = GetTask(contractPointer);
			return task;
		}

		public IReadOnlyList<CStoryContractConfig> GetConfigs(ERegion region)
		{
			return _configsByRegion[region];
		}
		
		protected void Build(CStoryContractsBuilder builder)
		{
			CStoryContractConfig[] contracts = builder.Build();
			_configsByRegion[builder.Region] = contracts;
			
			foreach (CStoryContractConfig contract in contracts)
			{
				_configsById.Add(contract.Id, contract);
			}
		}
	}
}