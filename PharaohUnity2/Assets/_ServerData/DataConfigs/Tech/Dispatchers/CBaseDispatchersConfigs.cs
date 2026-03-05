// =========================================
// AUTHOR: Juraj Joscak
// DATE:   25.11.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CBaseDispatchersConfigs
	{
		private readonly Dictionary<EDispatcher, CStaticDispatcherConfig> _configs = new();
		
		public CStaticDispatcherConfig GetConfig(EDispatcher dispatcher)
		{
			return _configs[dispatcher];
		}
		
		protected void AddDispatcher(EDispatcher dispatcherId, IUnlockRequirement unlockRequirement)
		{
			_configs.Add(dispatcherId, new CStaticDispatcherConfig(dispatcherId, unlockRequirement));
		}
	}
}