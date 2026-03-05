// =========================================
// AUTHOR: Radek Mikulka
// DATE:   12.09.2025
// =========================================

using AldaEngine;
using ServerData;

namespace TycoonBuilder
{
	public class CSetActiveContractDetailCameraSignal : IEventBusSignal
	{
		public readonly EStaticContractId ContractId;
		public readonly bool State;

		public CSetActiveContractDetailCameraSignal(EStaticContractId contractId, bool state)
		{
			ContractId = contractId;
			State = state;
		}
	}
}