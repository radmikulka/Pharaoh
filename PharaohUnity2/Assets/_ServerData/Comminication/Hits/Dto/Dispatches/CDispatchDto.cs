// =========================================
// AUTHOR: Radek Mikulka
// DATE:   11.07.2025
// =========================================

using AldaEngine;
using Newtonsoft.Json;

namespace ServerData.Dto
{
	public class CDispatchDto
	{
		[JsonProperty] public EDispatchType Type { get; set; }
		[JsonProperty] public string Uid { get; set; }
		[JsonProperty] public EVehicle VehicleId { get; set; }
		[JsonProperty] public long DispatchStartTime { get; set; }
		[JsonProperty] public long CompletionTime { get; set; }
		[JsonProperty] public long WaitTime { get; set; }

		[JsonProperty] public EStaticContractId Contract { get; set; }
		[JsonProperty] public int ResourceAmount { get; set; }

		[JsonProperty] public SResource ResourceToCollect { get; set; }
		[JsonProperty] public long TargetArrivalTime { get; set; }

		[JsonProperty] public ECity City { get; set; }

		[JsonProperty] public EVehicle[] FleetVehicles { get; set; }
		[JsonProperty] public EStaticContractId FleetContractId { get; set; }
		[JsonProperty] public string FleetContractUid { get; set; }

		public CDispatchDto()
		{
		}

		public CDispatchDto(
			EDispatchType type,
			string uid,
			EVehicle vehicleId,
			long dispatchStartTime,
			long completionTime,
			long waitTime,
			EStaticContractId contract,
			int resourceAmount,
			SResource resourceToCollect,
			long targetArrivalTime,
			ECity city,
			EVehicle[] fleetVehicles = null,
			EStaticContractId fleetContractId = default,
			string fleetContractUid = null
		)
		{
			Uid = uid;
			City = city;
			Type = type;
			Contract = contract;
			WaitTime = waitTime;
			VehicleId = vehicleId;
			FleetVehicles = fleetVehicles;
			CompletionTime = completionTime;
			ResourceAmount = resourceAmount;
			FleetContractId = fleetContractId;
			FleetContractUid = fleetContractUid;
			DispatchStartTime = dispatchStartTime;
			ResourceToCollect = resourceToCollect;
			TargetArrivalTime = targetArrivalTime;
		}
	}
}
