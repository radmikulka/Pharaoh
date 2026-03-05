// =========================================
// AUTHOR: Radek Mikulka
// DATE:   23.02.2026
// =========================================

using System.Collections.Generic;
using ServerData;

namespace TycoonBuilder
{
	public class CFleetDispatchData
	{
		public readonly EVehicle[] Vehicles;
		public readonly EStaticContractId ContractId;
		public readonly string ContractUid;
		private readonly Dictionary<EVehicle, CTrafficPath> _paths = new();

		public CFleetDispatchData(EVehicle[] vehicles, EStaticContractId contractId, string contractUid)
		{
			Vehicles = vehicles;
			ContractId = contractId;
			ContractUid = contractUid;
		}

		public void LoadVehiclePath(EVehicle vehicle, CTrafficPath path) => _paths[vehicle] = path;
		public CTrafficPath GetVehiclePath(EVehicle vehicle) => _paths[vehicle];
		public bool HasVehiclePath(EVehicle vehicle) => _paths.ContainsKey(vehicle);
	}
}
