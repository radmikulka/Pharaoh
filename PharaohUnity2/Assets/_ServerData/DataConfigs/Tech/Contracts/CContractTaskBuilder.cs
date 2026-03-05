// =========================================
// AUTHOR: Radek Mikulka
// DATE:   10.07.2025
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CContractTaskBuilder
	{
		private IContractTaskComponent _component;
		private bool _allowGoTo = true;
		private bool _closeDispatchMenuWhenIsFullyLoaded;
		private bool _closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle;
		private readonly List<IValuable> _rewards = new(2);

		public CContractTaskBuilder SetRequirement(EResource id, int amount)
		{
			_component = new CStandardContractTaskComponent(new SResource(id, amount));
			return this;
		}

		public CContractTaskBuilder AddReward(IValuable reward)
		{
			_rewards.Add(reward);
			return this;
		}

		public CContractTaskBuilder DisableGoTo()
		{
			_allowGoTo = false;
			return this;
		}

		public CContractTaskBuilder SetCloseDispatchMenuWhenTaskIsFullyLoaded()
		{
			_closeDispatchMenuWhenIsFullyLoaded = true;
			return this;
		}

		public CContractTaskBuilder SetCloseDispatchMenuWhenTaskIsFullyLoadedAndFollowVehicle()
		{
			_closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle = true;
			return this;
		}

		public CTransportFleetTaskConfigBuilder SetFleetTask(int totalPower)
		{
			return new CTransportFleetTaskConfigBuilder(this, totalPower);
		}

		internal void SetFleetComponent(CTransportFleetTaskConfig fleetConfig)
		{
			_component = fleetConfig;
		}

		public CContractTask Build()
		{
			return new CContractTask(
				_component,
				_rewards.ToArray(),
				_closeDispatchMenuWhenIsFullyLoaded,
				_closeDispatchMenuWhenIsFullyLoadedAndFollowVehicle,
				_allowGoTo
				);
		}
	}
}
