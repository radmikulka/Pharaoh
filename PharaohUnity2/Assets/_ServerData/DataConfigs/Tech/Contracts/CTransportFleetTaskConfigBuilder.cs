// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using System.Collections.Generic;

namespace ServerData
{
	public class CTransportFleetTaskConfigBuilder
	{
		private readonly CContractTaskBuilder _parent;
		private readonly List<CTransportFleetSlotConfig> _slots = new();
		private readonly int _totalPower;

		internal CTransportFleetTaskConfigBuilder(CContractTaskBuilder parent, int totalPower)
		{
			_totalPower = totalPower;
			_parent = parent;
		}

		public CTransportFleetTaskConfigBuilder AddSlot(EMovementType movementType, ETransportType transportType)
		{
			_slots.Add(new CTransportFleetSlotConfig(movementType, transportType));
			return this;
		}

		public CContractTaskBuilder Build()
		{
			_parent.SetFleetComponent(new CTransportFleetTaskConfig(_totalPower, _slots.ToArray()));
			return _parent;
		}
	}
}
