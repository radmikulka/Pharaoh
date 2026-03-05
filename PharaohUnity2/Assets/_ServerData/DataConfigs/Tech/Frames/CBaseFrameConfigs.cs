// =========================================
// AUTHOR: Radek Mikulka
// DATE:   26.06.2024
// =========================================

using System;
using System.Collections.Generic;

namespace ServerData
{
	public class CBaseFrameConfigs
	{
		private readonly Dictionary<EProfileFrame, CStaticFrameConfig> _configs = new();

		public IEnumerable<CStaticFrameConfig> GetAllConfigs()
		{
			foreach (var config in _configs)
			{
				yield return config.Value;
			}
		}
		
		public IEnumerable<CStaticFrameConfig> GetAllConfigs(Func<CStaticFrameConfig, bool> isValid)
		{
			foreach (var config in _configs)
			{
				if(isValid(config.Value))
				{
					yield return config.Value;
				}
			}
		}

		public IEnumerable<CStaticFrameConfig> GetAllConfigsUpToLevel(EYearMilestone yearMilestone)
		{
			foreach (var config in _configs)
			{
				
				if(config.Value.UnlockRequirement is CYearUnlockRequirement levelRequirement)
				{
					int requirement = (int)levelRequirement.Year;
					int currentYear = (int)yearMilestone;
					
					if (currentYear >= requirement)
					{
						yield return config.Value;
					}
				}
			}
		}

		public CStaticFrameConfig GetFrameConfigOrDefault(EProfileFrame id)
		{
			return _configs.GetValueOrDefault(id);
		}
		
		protected void AddFrame(EProfileFrame frameId, IUnlockRequirement unlockRequirement)
		{
			_configs.Add(frameId, new CStaticFrameConfig(frameId, unlockRequirement));
		}
	}
}