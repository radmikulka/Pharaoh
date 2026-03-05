// =========================================
// AUTHOR: Radek Mikulka
// DATE:   07.08.2025
// =========================================

using AldaEngine;
using ServerData;
using ServerData.Dto;

namespace TycoonBuilder
{
	public class CDispatch
	{
		public readonly EDispatchType Type;
		public readonly string Uid;
		public readonly EVehicle VehicleId;
		public readonly SUnixTime DispatchStartTime;
		public readonly SUnixTime TargetArrivalTime;
		public readonly SUnixTime WaitTime;
		public readonly SUnixTime CompletionTime;

		public SUnixTime TripCompletionTime => Type == EDispatchType.Resource ? CompletionTime : TargetArrivalTime + WaitTime;

		public CTrafficPath Path { get; private set; }

		public readonly CContractDispatchData ContractData;
		public readonly CPassengerDispatchData PassengerData;
		public readonly CResourceDispatchData ResourceData;
		public readonly CFleetDispatchData FleetData;

		public CDispatch(
			EDispatchType type,
			string uid,
			EVehicle vehicleId,
			SUnixTime dispatchStartTime,
			SUnixTime targetArrivalTime,
			SUnixTime waitTime,
			SUnixTime completionTime,
			CContractDispatchData contractData = null,
			CPassengerDispatchData passengerData = null,
			CResourceDispatchData resourceData = null,
			CFleetDispatchData fleetData = null
		)
		{
			Type = type;
			Uid = uid;
			VehicleId = vehicleId;
			WaitTime = waitTime;
			DispatchStartTime = dispatchStartTime;
			TargetArrivalTime = targetArrivalTime;
			CompletionTime = completionTime;
			ContractData = contractData;
			PassengerData = passengerData;
			ResourceData = resourceData;
			FleetData = fleetData;
		}

		public CDispatch(CDispatchDto dto)
		{
			Type = dto.Type;
			Uid = dto.Uid;
			VehicleId = dto.VehicleId;
			DispatchStartTime = dto.DispatchStartTime;
			TargetArrivalTime = dto.TargetArrivalTime;
			CompletionTime = dto.CompletionTime;
			WaitTime = dto.WaitTime;

			switch (dto.Type)
			{
				case EDispatchType.Contract:
					ContractData = new CContractDispatchData(dto.Contract, dto.ResourceAmount);
					break;
				case EDispatchType.Passenger:
					PassengerData = new CPassengerDispatchData(dto.City, dto.ResourceAmount);
					break;
				case EDispatchType.Resource:
					ResourceData = new CResourceDispatchData(dto.ResourceToCollect);
					break;
				case EDispatchType.TransportFleet:
					FleetData = new CFleetDispatchData(dto.FleetVehicles, dto.FleetContractId, dto.FleetContractUid);
					break;
			}
		}

		public bool IsCompleted(long currentTime)
		{
			return currentTime >= CompletionTime;
		}
		
		public bool IsTripCompleted(long currentTime)
		{
			return currentTime >= TripCompletionTime;
		}

		public void LoadPath(CTrafficPath path)
		{
			Path = path;
		}
	}
}
